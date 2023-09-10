The examples below are mostly for **InterpolatedSql.Dapper**. If you're using just the core library (**InterpolatedSql**, without the Dapper extension) you'll have minor differences (no facades to invoke Dapper methods, and no extensions to create the builders on top of a IDbConnection).

# SqlBuilder (Basics)

SqlBuilder is the most simple builder and can be used for any purpose. It just renders the statement that you explicitly append - it doesn't modify the generated statement.  

Most of the methods (like `Append*`, and also the operator overloads that make `+` behave like `Append()`) are defined in the base class `InterpolatedSqlBuilderBase<U,R>`, and will return the same type (even if you create your own subclass - check [Advanced](Advanced.md)).

Most of the methods that you'll se below for `SqlBuilder` will also work for `QueryBuilder` (which is just an specialization), so in examples below you can probably just replace `SqlBuilder` by `QueryBuilder`.

## Static Query

```cs
using InterpolatedSql.Dapper;
// ...

cn = new SqlConnection(connectionString);

// Build your query with interpolated parameters
// which are automagically converted into safe SqlParameters
var products = cn.SqlBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE ListPrice <= {maxPrice}
    AND Weight <= {maxWeight}
    AND Name LIKE {search}
    ORDER BY ProductId").Query<Product>();
```

So, basically you pass parameters as interpolated strings, but they are converted to safe SqlParameters.

This is our mojo :-) 

## Dynamic Query

```cs
using InterpolatedSql.Dapper;
// ...

cn = new SqlConnection(connectionString);

// Build initial query
var q = cn.SqlBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE 1=1");

// and dynamically append extra filters
q += $"AND ListPrice <= {maxPrice}";
q += $"AND Weight <= {maxWeight}";
q += $"AND Name LIKE {search}";
q += $"ORDER BY ProductId";

var products = q.Build().Query<Product>();
// all other Dapper extensions are also available: QueryAsync, QueryMultiple, ExecuteScalar, etc..
```


## Static Command

```cs
var cmd = cn.SqlBuilder($"DELETE FROM Orders WHERE OrderId = {orderId};");
int deletedRows = cmd.Execute();
```

```cs
cn.SqlBuilder($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ({productName}, {ProductSubcategoryID})
").Execute();
```


## Command with Multiple statements

In a single roundtrip we can run multiple SQL commands:

```cs
var cmd = cn.SqlBuilder();
cmd += $"DELETE FROM Orders WHERE OrderId = {orderId}; ";
cmd += $"INSERT INTO Logs (Action, UserId, Description) VALUES ({action}, {orderId}, {description}); ";
cmd.Execute();
```


## Inner Queries

It's possible to add injection-safe queries inside other queries (e.g. to use as subqueries).
This makes it easier to break very complex queries into smaller methods/blocks, or reuse queries as subqueries.
The parameters are fully preserved and safe:

```cs
int orgId = 123;
var innerQuery = cn.SqlBuilder($"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}");
var q = cn.QueryBuilder($@"
    SELECT FROM ({innerQuery}) a 
    JOIN AnotherTable b ON a.Id=b.Id 
    WHERE b.OrganizationId={321}")
    .Build();

// q.Sql is like:
// SELECT FROM (SELECT Id, Name FROM SomeTable where OrganizationId=@p0) a 
// JOIN AnotherTable b ON a.Id=b.Id 
// WHERE b.OrganizationId=@p1"
```

If you prefer to use standard types for your subqueries, just declare them as FormattableString (to preserve the isolation between SQL statement and SQL Parameters):

```cs
FormattableString innerQuery = $"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}"; // do not use "var" !
var q = cn.QueryBuilder($@"
    SELECT FROM ({innerQuery}) a
    ...").Build();
```

## IN lists

Dapper allows us to use IN lists magically. And it also works with our string interpolation:

```cs
var q = cn.QueryBuilder($@"
    SELECT c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");

var categories = new string[] { "Components", "Clothing", "Acessories" };
q += $"WHERE c.Name IN {categories}";
```




# QueryBuilder

This is a builder with some helpers to build queries (SELECT statements) with **dynamic filters**.


## Dynamic Query with /\*\*where\*\*/ keyword

If you don't like the idea of using `WHERE 1=1` (even though it [doesn't hurt performance](https://dba.stackexchange.com/a/33958/85815)), you can use the special keyword **/\*\*where\*\*/** that act as a placeholder to render dynamically-defined filters.  

`QueryBuilder` maintains an internal list of filters (property called `Filters`) which keeps track of all filters you've added using `.Where()` method.
Then, when `QueryBuilder` invokes Dapper and sends the underlying query it will search for the keyword `/**where**/` in our query and if it exists it will replace it with the filters added (if any), combined using `AND` statements.


Example: 

```cs
// We can write the query structure and use QueryBuilder to render the "where" filters (if any)
var q = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");
    
// You just pass the parameters as if it was an interpolated string, 
// and QueryBuilder will automatically convert them to Dapper parameters (injection-safe)
q.Where($"ListPrice <= {maxPrice}");
q.Where($"Weight <= {maxWeight}");
q.Where($"Name LIKE {search}");

// Query() will automatically render your query and replace /**where**/ keyword (if any filter was added)
var products = q.Build().Query<Product>();

// In this case Dapper would get "WHERE ListPrice <= @p0 AND Weight <= @p1 AND Name LIKE @p2" and the associated values
```

When Dapper is invoked we replace the `/**where**/` by `WHERE <filter1> AND <filter2> AND <filter3...>` (if any filter was added).

## Dynamic Query with /\*\*filters\*\*/ keyword

**/\*\*filters\*\*/** is exactly like **/\*\*where\*\*/**, but it's used if we already have other fixed conditions before:
```cs
var q = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE Price>{minPrice} /**filters**/
    ORDER BY ProductId
    ");
```

When Dapper is invoked we replace the `/**filters**/` by `AND <filter1> AND <filter2...>` (if any filter was added).


## Writing complex filters (combining AND/OR Filters) and using the Filters class

As explained above, `QueryBuilder` internally contains an instance of `Filters` class, which basically contains a list of filters and a combining operator (default is AND but can be changed to OR).
These filters are defined using `.Where()` and are rendered through the keywords `/**where**/` or `/**filters**/`.

Each filter (inside a parent list of `Filters`) can be a simple condition (using interpolated strings) or it can recursively be another list of filters (`Filters` class), 
and this can be used to write complex combinations of AND/OR conditions (inner filters filters are grouped by enclosing parentheses):

```cs
var q = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");

var priceFilters = new Filters(Filters.FiltersType.OR)
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
};
// Or we could add filters one by one like:  priceFilters.Add($"Weight <= {maxWeight}");

q.Where("Status={status}");
// /**where**/ would be replaced by "Status=@p0"

q.Where(priceFilters);
// /**where**/ would be replaced as "Status=@p0 AND (ListPrice >= @p1 OR ListPrice <= @p2)".
// Note that priceFilters is an inner Filter and it's enclosed with parentheses

// It's also possible to change the combining operator of the outer query or of inner filters:
// q.FiltersType = Filters.FiltersType.OR;
// priceFilters.FiltersType = Filters.FiltersType.AND;
// /**where**/ would be replaced as "Status=@p0 OR (ListPrice >= @p1 AND ListPrice >= @p2)".

var products = q.Build().Query<Product>();
```

To sum, `Filters` class will render whatever conditions you define, conditions can be combined with `AND` or `OR`, and conditions can be defined as inner filters (will use parentheses).
This is all vendor-agnostic (`AND`/`OR`/parentheses are all SQL ANSI) so it should work with any vendor.



# FluentQueryBuilder 

FluentQueryBuilder uses a Fluent API (chained methods) to let users build step-by-step a syntatically-valid query. Each invoked method will return different interfaces - and those interfaces will only offer the methods that are valid in each stage (e.g. initially it only accepts SELECTs, then it expects FROMs, then you can add some WHEREs, optionally some GROUP BY, HAVING, and finally ORDER BY).

So, basically, instead of starting with a full query and just appending new filters (`.Where()`), the FluentQueryBuilder will build the whole query for you:

```cs
var q = cn.FluentQueryBuilder()
    .Select($"ProductId")
    .Select($"Name")
    .Select($"ListPrice")
    .Select($"Weight")
    .From($"Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy($"ProductId");
    
var products = q.Build().Query<Product>();
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM Product
WHERE ListPrice <= @p0 AND Weight <= @p1 AND Name LIKE @p2
ORDER BY ProductId
```
Or more elaborated:

```cs
var q = cn.FluentQueryBuilder()
    .SelectDistinct($"ProductId, Name, ListPrice, Weight")
    .From("Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy("ProductId");
```

Building joins dynamically using Fluent API:

```cs
var categories = new string[] { "Components", "Clothing", "Acessories" };

var q = cn.FluentQueryBuilder()
    .SelectDistinct($"c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber")
    .From($"Product p")
    .From($"INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID")
    .From($"INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID")
    .Where($"c.Name IN {categories}");
```

There are also chained-methods for adding GROUP BY, HAVING, ORDER BY, and paging (OFFSET x ROWS / FETCH NEXT x ROWS ONLY).


# Advanced feature: Raw Modifier

When we want to use regular string interpolation for building up our queries/commands but the interpolated values are not supposed to be converted into SQL parameters we can use the **raw modifier** (works with any builder). See some examples below.

## Dynamic Column Names (Raw Modifier)

```cs
var query = connection.QueryBuilder($"SELECT * FROM Employee WHERE 1=1");
foreach(var filter in filters)
    query += $" AND {filter.ColumnName:raw} = {filter.Value}";
```

Or:

```cs
var query = connection.QueryBuilder($"SELECT * FROM Employee /**where**/");
foreach(var filter in filters)
    query.Where($"{filter.ColumnName:raw} = {filter.Value}");
```

Whatever we pass as `:raw` should be either "trusted" or if it's untrusted (user-input) it should be sanitized correctly to avoid SQL-injection issues. (e.g. if `filter.ColumnName` comes from the UI we should validate it or sanitize it against SQL injection).


## nameof (using Raw modifier)
Another example of using the **raw modifier** is when we want to use **nameof expression** (which allows to "find references" for a column, "rename", etc):

```cs
var q = cn.QueryBuilder($@"
    SELECT
        c.{nameof(Category.Name):raw} as Category, 
        sc.{nameof(Subcategory.Name):raw} as Subcategory, 
        p.{nameof(Product.Name):raw}, p.ProductNumber"
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");
```

## Dynamic Table Names (Raw Modifier)

Another common use for **raw modifier** is when we're creating a global temporary table and want a unique (random) name:

```cs
string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
string name = "Rick";

cn.QueryBuilder($@"
    CREATE TABLE ##tmpTable{uniqueId:raw}
    (
        Name nvarchar(200)
    );
    INSERT INTO ##tmpTable{uniqueId:raw} (Name) VALUES ({name});
").Execute();
```

Let's emphasize again: strings that you interpolate using `:raw` modifier are not passed as parameters and therefore you should ensure validade it or sanitize it against SQL injection.



# Advanced features

Some more advanced features that work with any builder.


## Explicitly describing varchar/char data types (to avoid varchar performance issues)

For Dapper (and consequently for us) strings are always are assumed to be unicode strings (nvarchar) by default.  

This causes a [known Dapper issue](https://jithilmt.medium.com/sql-server-hidden-load-evil-performance-issue-with-dapper-465a08f922f6): If the column datatype is varchar the query may not give the best performance and may even ignore existing indexed columns and do a full table scan.  
So for achieving best performance we may want to explicitly describe if our strings are unicode (nvarchar) or ansi (varchar), and also describe their exact sizes.

Dapper's solution is to use the `DbString` class as a wrapper to describe the data type more explicitly, and QueryBuilder can also take this `DbString` in the interpolated values:

```cs
string productName = "Mountain%";

// This is how we declare a varchar(50) in plain Dapper
var productVarcharParm = new DbString { 
    Value = productName, 
    IsFixedLength = true, 
    Length = 50, 
    IsAnsi = true 
};

// Our builders understand Dapper DbString:
var query = cn.QueryBuilder($@"
    SELECT * FROM Production.Product p 
    WHERE Name LIKE {productVarcharParm}");
```

**But we can also specify the datatype (using the well-established SQL syntax) after the value (`{value:datatype}`):**

```cs
string productName = "Mountain%";

var query = cn.QueryBuilder($@"
    SELECT * FROM Production.Product p 
    WHERE Name LIKE {productName:varchar(50)}");
```

The library will parse the datatype specified after the colon, and it understands sql types like `varchar(size)`, `nvarchar(size)`, `char(size)`, `nchar(size)`, `varchar(MAX)`, `nvarchar(MAX)`.  

`nvarchar` and `nchar` are unicode strings, while `varchar` and `char` are ansi strings.  
`nvarchar` and `varchar` are variable-length strings, while `nchar` and `char` are fixed-length strings.

Don't worry if your database does not use those exact types - we basically convert from those formats back into Dapper `DbString` class (with the appropriate hints `IsAnsi` and `IsFixedLength`), and Dapper will convert that to your database.

## ValueTuple

Dapper allows us to map rows to ValueTuples. And it also works with our string interpolation:

```cs
// Sometimes we don't want to declare a class for a simple query
var q = cn.QueryBuilder($@"
    SELECT Name, ListPrice, Weight
    FROM Product
    WHERE ProductId={productId}");
var productDetails = q.Build().QuerySingle<(string Name, decimal ListPrice, decimal Weight)>();
```

Warning: Dapper Tuple mapping is based on positions (it's not possible to map by names)




# Some extra string magic we have (in all builders)

**Automatic spacing:**
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var products = query.Build().Query<Product>(); 
```

No need to worry about adding a space before or after a new clause. We'll handle that for you


**Automatic strip of surrounding single-quotes**:

If by mistake you add single quotes around interpolated arguments (as if it was dynamic SQL) we'll just strip it for you.

```cs
cn.SqlBuilder($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ('{productName}', '{ProductSubcategoryID}')
").Build().Execute();
// Dapper will get "... VALUES (@p0, @p1) " (we'll remove the surrounding single quotes)
```

```cs
string productName = "%Computer%";
var products = cnQueryBuilder($"SELECT * FROM Product WHERE Name LIKE '{productName}'");
// Dapper will get "... WHERE Name LIKE @p0 " (we'll remove the surrounding single quotes)
```

**Automatic reuse of duplicated parameters**:

If you use the same value twice in the query we'll just pass it once and reuse the existing parameter.

```cs
InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters = true; // default is false
string productName = "Computer";
var products = cn.QueryBuilder($"SELECT * FROM Product WHERE Name = {productName} OR Category = {productName}").Build().Query<Product>;
// Dapper will get "... WHERE Name = @p0 OR Category = @p0 " (we'll send @p0 only once)
```

**Automatic trimming for multi-line queries**:
```cs

InterpolatedSqlBuilderOptions.DefaultOptions.AutoAdjustMultilineString = true; // default is false

var products = cn
    .QueryBuilder($@"
    SELECT * FROM Product
    WHERE
    Name LIKE {productName}
    AND ProductSubcategoryID = {subCategoryId}
    ORDER BY ProductId").Build().Query<Product>;
```

Since this is a multi-line interpolated string we'll automatically trim the first empty line and "dock to the left"  (remove left padding). What Dapper receives does not have whitespace, making it easier for logging or debugging:
```sql
SELECT * FROM Product
WHERE
Name LIKE @p0
AND ProductSubcategoryID = @p1
ORDER BY ProductId
``` 


## Invoking Stored Procedures
```cs
// This is basically Dapper, but with a FluentAPI where you can append parameters dynamically.
var cmd = cn.SqlBuilder($"HumanResources.uspUpdateEmployeePersonalInfo")
    .AddParameter("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue)
    .AddParameter("ErrorLogID", dbType: DbType.Int32, direction: ParameterDirection.Output)
    .AddParameter("BusinessEntityID", businessEntityID)
    .AddParameter("NationalIDNumber", nationalIDNumber)
    .AddParameter("BirthDate", birthDate)
    .AddParameter("MaritalStatus", maritalStatus)
    .AddParameter("Gender", gender)
    .Build();
    
int affected = cmd.Execute(commandType: CommandType.StoredProcedure);
int returnValue = cmd.Parameters.Get<int>("ReturnValue");
```



<!-- ## Using Type-Safe Filters without QueryBuilder

If you want to use our type-safe dynamic filters but for some reason you don't want to use our QueryBuilder:

```cs
Dapper.DynamicParameters parms = new Dapper.DynamicParameters();

var filters = new Filters(Filters.FiltersType.AND);
filters.Add(new Filters()
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
});
filters.Add(new Filters(Filters.FiltersType.OR)
{
    new Filter($"Weight <= {maxWeight}"),
    new Filter($"Name LIKE {search}")
});

string where = filters.BuildFilters(parms);
// "WHERE (ListPrice >= @p0 AND ListPrice <= @p1) AND (Weight <= @p2 OR Name LIKE @p3)"
// parms contains @p0 as minPrice, @p1 as maxPrice, etc..
``` -->


