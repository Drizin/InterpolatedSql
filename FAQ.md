# FAQ

## Why should we always use Parameterized Queries instead of Dynamic SQL?

The whole purpose of Dapper is to safely map our objects to the database (and to map database records back to our objects).  
If you build SQL statements by concatenating parameters as strings it means that:

- It would be more vulnerable to SQL injection.
- You would have to manually sanitize your inputs [against SQL-injection attacks](https://stackoverflow.com/a/7505842)
- You would have to manually convert null values
- Your queries wouldn't benefit from cached execution plan
- You would go crazy by not using Dapper like it was supposed to be used

Building dynamic SQL (**which is a TERRIBLE idea**) would be like this:

```cs 
string sql = "SELECT * FROM Product WHERE Name LIKE " 
   + "'" + productName.Replace("'", "''") + "'"; 
// now you pray that you've correctly sanitized inputs against sql-injection
var products = cn.Query<Product>(sql);
```

With plain Dapper it's safer:
```cs
string sql = "SELECT * FROM Product WHERE Name LIKE @productName";
var products = cn.Query<Product>(sql, new { productName });
``` 


**But with Dapper Query Builder it's even easier**:
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE Name LIKE {productName}");
var products = query.Build().Query<Product>(); 
```



## Why this library if we could just use interpolated strings directly with plain Dapper?

Dapper does not take interpolated strings, and therefore it doesn't do any kind of manipulation magic on interpolated strings (which is exactly what we do).  
This means that if you pass an interpolated string to Dapper it will be converted as a plain string (**so it would run as dynamic SQL, not as parameterized SQL**), meaning it has **the same issues as dynamic sql** (see previous question).  

So it WOULD be possible (but ugly) to use Dapper with interpolated strings (as long as you sanitize the inputs):

```cs
cn.Execute($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ( 
      '{productName.Replace("'", "''")}', 
      {ProductSubcategoryID == null ? "NULL" : int.Parse(ProductSubcategoryID).ToString()}
   )");
// now you pray that you've correctly sanitized inputs against sql-injection
```

But with our library this is not only safer but also much simpler:

```cs
cn.SqlBuilder($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ({productName}, {ProductSubcategoryID})
").Build().Execute();
```

In other words, passing interpolated strings to Dapper is dangerous because you may forget to sanitize the inputs.  

Our library makes the use of interpolated strings easier and safer because:
- You don't have to sanitize the parameters (we rely on Dapper parameters)
- You don't have to convert from nulls (we rely on Dapper parameters)
- Our methods will never accept plain strings to avoid programmer mistakes
- If you want to embed in the interpolated statement a regular string a do NOT want it to be converted to a parameter you need to explicitly describe it with the `:raw` modifier

## Why do I have to write everything using interpolated strings (`$`)

The magic is that when you write an interpolated string our methods can identify the embedded parameters (interpolated values) and can correctly extract them and parameterize them.  
By enforcing that all methods only take `FormattableString` we can be confident that our methods will never receive an implicit conversion to string, which would defeat the whole purpose of the library and would make you vulnerable to SQL injection.  
The only way you can pass an unsafe string in your interpolation is if you explicitly add the **`:raw` modifier**, so it's easy to review all statements for vulnerabilities.  
As Alan Kay says, "Simple things should be simple and complex things should be possible" - so interpolating regular sql parameters is very simple, while interpolating plain strings is still possible.

## Is building queries with string interpolation really safe?

In our library String Interpolation is just an abstraction used for hiding the complexity of manually creating SqlParameters.  
This library is as safe as possible because it never accepts plain strings, so there's no risk of accidentally converting an interpolated string into a vulnerable string. But obviously there are a few possible scenarios where mistakes could happen.

**First possible mistake - using raw modifier for things that should be parameters:**

```cs
using InterpolatedSql.Dapper;

// If you don't understand what raw is for, DON'T USE IT - code below is unsafe!
var products = cn.QueryBuilder($@"
    SELECT * FROM Product WHERE ProductId={productId:raw}"
).Query<Product>();
```

**Second possible mistake - passing interpolated strings to Dapper instead of InterpolatedSql.Dapper:**

```cs
using Dapper;

// UNSAFE CODE. Database will receive an unsafe (not parameterized) query.
var products = cn.Query<Product>($@"
    SELECT * FROM Product WHERE ProductId={productId}"
);

// To avoid this type of mistake you can just avoid Dapper namespace
// and just use "using InterpolatedSql.Dapper;"
```


## How can I use Dapper async extensions?

This documentation is mostly using sync methods, but we have [facades](/src/InterpolatedSql.Dapper/IDapperSqlCommandExtensions.cs) for **all** Dapper extensions, including `ExecuteAsync()`, `QueryAsync<T>`, etc. 

## How can I use Dapper Multi-Mapping?

We do not have facades for invoking Dapper Multi-mapper extensions directly, but you can combine QueryBuilder with Multi-mapper extensions by explicitly passing builder `.Sql` and `.Parameters`:

```cs
// InterpolatedSql.Dapper QueryBuilder
var orderAndDetailsQuery = cn
    .QueryBuilder($@"
    SELECT * FROM Orders AS A 
    INNER JOIN OrderDetails AS B ON A.OrderID = B.OrderID
    /**where**/
    ORDER BY A.OrderId, B.OrderDetailId");
// Dynamic Filters
orderAndDetailsQuery.Where($"[ListPrice] <= {maxPrice}");
orderAndDetailsQuery.Where($"[Weight] <= {maxWeight}");
orderAndDetailsQuery.Where($"[Name] LIKE {search}");

// Dapper Multi-mapping extensions
var orderAndDetails = cn.Query<Order, OrderDetail, Order>(
        /* orderAndDetailsQuery.Build().Sql contains [ListPrice] <= @p0 AND [Weight] <= @p1 etc... */
        sql: orderAndDetailsQuery.Build().Sql,
        map: (order, orderDetail) =>
        {
            // whatever..
        },
        /* orderAndDetailsQuery.Build().Parameters contains @p0, @p1 etc... */
        param: orderAndDetailsQuery.Build().Parameters,
        splitOn: "OrderDetailID")
    .Distinct()
    .ToList();
```

To sum, instead of using InterpolatedSql.Dapper `.Query<T>` extensions (which invoke Dapper `IDbConnection.Query<T>`) you just invoke Dapper multimapper `.Query<T1, T2, T3>` directly, and use InterpolatedSql.Dapper only for dynamically building your filters.  

## How does InterpolatedSql.Dapper compare to plain Dapper?

**Building dynamic filters in plain Dapper is a little cumbersome / ugly:**
```cs
var parms = new DynamicParameters();
List<string> filters = new List<string>();

filters.Add("Name LIKE @productName"); 
parms.Add("productName", productName);
filters.Add("CategoryId = @categoryId"); 
parms.Add("categoryId", categoryId);

string where = (filters.Any() ? " WHERE " + string.Join(" AND ", filters) : "");

var products = cn.Query<Product>($@"
    SELECT * FROM Product
    {where}
    ORDER BY ProductId", parms);
```

**With InterpolatedSql.Dapper it's much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder($@"
    SELECT * FROM Product 
    /**where**/ 
    ORDER BY ProductId");

query.Where($"Name LIKE {productName}");
query.Where($"CategoryId = {categoryId}");

// If any filter was added, Query() will automatically replace the /**where**/ keyword
var products = query.Build().Query<Product>();
```

or without `/**where**/`:
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}";
query += $"AND CategoryId = {categoryId}";
query += $"ORDER BY ProductId";
var products = query.Build().Query<Product>();
```


## How does InterpolatedSql.Dapper compare to [Dapper.SqlBuilder](https://github.com/DapperLib/Dapper/tree/main/Dapper.SqlBuilder)?

Dapper.SqlBuilder combines the filters using `/**where**/` keyword (like we do) but requires some auxiliar classes, and filters have to be defined using Dapper syntax (no string interpolation):

```cs
// SqlBuilder and Template are helper classes
var builder = new SqlBuilder();

// We also use this /**where**/ syntax
var template = builder.AddTemplate(@"
    SELECT * FROM Product
    /**where**/
    ORDER BY ProductId");
    
// Define the filters using regular Dapper syntax:
builder.Where("Name LIKE @productName", new { productName });
builder.Where("CategoryId = @categoryId", new { categoryId });

// Use template to explicitly pass the rendered SQL and parameters to Dapper:
var products = cn.Query<Product>(template.RawSql, template.Parameters);
```


## Why don't you create Typed Filters using Lambda Expressions?

SQL syntax is powerful, comprehensive and vendor-specific. Dapper allows us to use the full power of SQL (with database vendor specific features), and so does InterpolatedSql.Dapper.  
For those reasons this library is focused only in converting interpolated strings into SQL parameters, instead of trying to reinvent SQL syntax or create a limited abstraction over SQL language.  
If you want a full fledged ORM with typed filters just use Entity Framework.