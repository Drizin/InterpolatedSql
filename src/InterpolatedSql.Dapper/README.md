[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql?label=InterpolatedSql.Dapper)](https://www.nuget.org/packages/InterpolatedSql.Dapper)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.Dapper.svg)](https://www.nuget.org/packages/InterpolatedSql.Dapper)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql.Dapper.StrongName?label=InterpolatedSql.Dapper.StrongName)](https://www.nuget.org/packages/InterpolatedSql.Dapper.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.Dapper.StrongName.svg)](https://www.nuget.org/packages/InterpolatedSql.Dapper.StrongName)


# InterpolatedSql.Dapper

[**InterpolatedSql**](https://github.com/Drizin/InterpolatedSql) is a .NET library to build injection-safe SQL statements using pure string interpolation, but it does not provide any database-specific code.  


**InterpolatedSql.Dapper** library extends the InterpolatedSql core library by adding Dapper integration and by mapping Dapper types.

**Please read the [InterpolatedSql documentation](https://github.com/Drizin/InterpolatedSql) before going further.**


# How it works

The library provides a few different **SQL Builders** - these classes contain an underlying **SQL Statement** (text) and also the associated **SQL Parameters**.  

When those builders are created (or when we append to them) they will automatically parse interpolated strings and will extract the **injection-safe SQL statement** and the **interpolated objects** (SQL Parameters), keeping them isolated from each other.  

In other words, you just embed parameters inside the SQL text and the library will automatically capture the parameters (and keep them isolated from the text), providing an injection-safe SQL statement.

**InterpolatedSql.Dapper** extends the SQL Builders of the base library with Dapper-specific features:
- They wrap a required `IDbConnection DbConnection`
- They are created using "Dapper-style": there are extension methods that extend IDbConnection ( exactly like Dapper does) that are used to create the builders
- After you `Build()` a SQL builder you can invoke facades to all Dapper extensions.

# Quickstart and Examples

1. Install the [NuGet package InterpolatedSql.Dapper](https://www.nuget.org/packages/InterpolatedSql.Dapper) or [NuGet package InterpolatedSql.Dapper.StrongName](https://www.nuget.org/packages/InterpolatedSql.Dapper.StrongName)
1. Start using like examples below


**Query:**

```cs
using InterpolatedSql.Dapper;
// ...

int categoryId = 10;
var cn = new SqlConnection(connectionString);

// Build() will return an object of type InterpolatedSql.Dapper.IDapperSqlCommand
var query = cn.SqlBuilder($"SELECT * FROM Product WHERE CategoryId={categoryId}")
    .Build();
// it will have these values:
// query.Sql == "SELECT * FROM Product WHERE CategoryId=@p0"
// query.Parameters["p0"].Value == 10

// There are extensions (over IDapperSqlCommand) to call all Dapper methods:
var product = query.Query<Product>();
// When .Query<T>() is invoked it  will invoke the equivalent Dapper extension and will pass a fully parameterized query that you see above (no risk of SQL-injection)
```

**Command:**

```cs
using InterpolatedSql.Dapper;
// ...

int categoryId = 10;
string productName = "Computer";
decimal price = 10.30;
var cn = new SqlConnection(connectionString);

var cmd = cn.SqlBuilder($"INSERT INTO Product (CategoryId, ProductName, Price) VALUES ({categoryId}, {productName}, {price})")
    .Build();
cmd.Execute();

// cmd.Sql == "INSERT INTO Product (CategoryId, ProductName, Price) VALUES (@p0, @p1, @p2)"
// cmd.Parameters.Count == 3
// cmd.Parameters["p0"].Value == 10
// cmd.Parameters["p1"].Value == "Computer"
// cmd.Parameters["p2"].Value == 10.30;
```

**Multiline:**

```cs
using InterpolatedSql.Dapper;
// ...

string productName = "%Laptop%";
var cn = new SqlConnection(connectionString);

var query = cn.SqlBuilder($@"
    SELECT * FROM Product
    WHERE
    CategoryID = {categoryId}
    AND Price <= {price}
    AND Name LIKE {productName}
    ORDER BY ProductId"
    ).Build();
var product = query.Query<Product>();

/*
System.Diagnostics.Debug.WriteLine(query.Sql);

    SELECT * FROM Product
    WHERE
    CategoryID = @p0
    AND Price <= @p1
    AND Name LIKE @p2
    ORDER BY ProductId
*/
```

**Dynamic Query:**

SQL Builders wrap two things that should always stay together: the query which you're building, and the parameters that must go together with our query.
This is a simple concept but it allows us to dynamically add new parameterized SQL clauses/conditions in a single statement:

```cs
string productName = "%Computer%";
int subCategoryId = 10;
var cn = new SqlConnection(connectionString);

var query = cn.SqlBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var product = query.Build().Query<Product>();
```

(If it wasn't for a single structure wrapping both the SQL text and the SQL Parameters, you would have to maintain two independent objects - a StringBuilder and a parameters Dictionary).



# How is this any different than using plain Dapper?

Building a dynamic query using **plain Dapper**:

```cs
using Dapper;
//...

// Parameters and SQL text are independent objects
var dynamicParams = new DynamicParameters();
string sql = "SELECT * FROM Product WHERE 1=1";
sql += " AND Name LIKE @productName"; 
dynamicParams.Add("productName", productName);
sql += " AND ProductSubcategoryID = @subCategoryId"; 
dynamicParams.Add("subCategoryId", subCategoryId);
var products = cn.Query<Product>(sql, dynamicParams);
``` 

Building a dynamic query using **InterpolatedSql.Dapper** is much easier:

```cs
using InterpolatedSql.Dapper;
// ...

var query = cn.SqlBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var products = query.Build().Query<Product>();
```

# Database Support

QueryBuilder is database agnostic (like Dapper) - it should work with all ADO.NET providers (including Microsoft SQL Server, PostgreSQL, MySQL, SQLite, Firebird, SQL CE and Oracle), since it's basically a wrapper around the way parameters are passed to the database provider.  

DapperQueryBuilder doesn't generate SQL statements (except for simple clauses which should work in all databases like `WHERE`/`AND`/`OR` - if you're using `/**where**/` keyword).  


It was tested with **Microsoft SQL Server** and with **PostgreSQL** (using Npgsql driver), and works fine in both.  

## Parameters prefix

By default the parameters are generated using "at-parameters" format (the first parameter is named `@p0`, the next is `@p1`, etc), and that should work with most database providers (including PostgreSQL Npgsql).  
If your provider doesn't accept at-parameters (like Oracle) you can modify `DapperQueryBuilderOptions.DatabaseParameterSymbol`:

```cs
// Default database-parameter-symbol is "@", which mean the underlying query will use @p0, @p1, etc..
// Some database vendors (like Oracle) expect ":" parameters instead of "@" parameters
DapperQueryBuilderOptions.DatabaseParameterSymbol = ":";

OracleConnection cn = new OracleConnection("DATA SOURCE=server;PASSWORD=password;PERSIST SECURITY INFO=True;USER ID=user");

string search = "%Dinosaur%";
var cmd = cn.QueryBuilder($"SELECT * FROM film WHERE title like {search}");
// Underlying SQL will be: SELECT * FROM film WHERE title like :p0
```

If for any reason you don't want parameters to be named `p0`, `p1`, etc, you can change the auto-naming prefix by setting `AutoGeneratedParameterName`:

```cs
DapperQueryBuilderOptions.AutoGeneratedParameterName = "PARAM_";

// your parameters will be named @PARAM_0, @PARAM_1, etc..
```

To learn more about the different builders and their features, check the [Builders](/Builders.md) page.

To make advanced customizations using this library, check the [Advanced](/Advanced.md) page.

For other questions check our [FAQ](/FAQ.md) page.


# About

## Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/InterpolatedSql&type=Date)](https://star-history.com/#Drizin/InterpolatedSql&Date)


## License
MIT License


