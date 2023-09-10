[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql?label=InterpolatedSql)](https://www.nuget.org/packages/InterpolatedSql)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.svg)](https://www.nuget.org/packages/InterpolatedSql)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql.StrongName?label=InterpolatedSql.StrongName)](https://www.nuget.org/packages/InterpolatedSql.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.StrongName.svg)](https://www.nuget.org/packages/InterpolatedSql.StrongName)

[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql?label=InterpolatedSql.Dapper)](https://www.nuget.org/packages/InterpolatedSql.Dapper)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.Dapper.svg)](https://www.nuget.org/packages/InterpolatedSql.Dapper)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql.Dapper.StrongName?label=InterpolatedSql.Dapper.StrongName)](https://www.nuget.org/packages/InterpolatedSql.Dapper.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.Dapper.StrongName.svg)](https://www.nuget.org/packages/InterpolatedSql.Dapper.StrongName)


# Interpolated Sql

**InterpolatedSql is a .NET library to build SQL statements (queries or commands) using pure string interpolation.**

It provides a friendly and intuitive syntax that can be used to **dynamically** write complex SQL queries (or SQL commands).

# How it works

The library provides a few different **SQL Builders** - those classes contain an underlying **SQL Statement** (text) and also the associated **SQL Parameters**.  

When those builders are created (or when we append to them) they will automatically parse interpolated strings and will extract the **injection-safe SQL statement** and the **interpolated objects** (SQL Parameters), keeping them isolated from each other.  

In other words, you just embed parameters inside the SQL text and the library will automatically capture the parameters (and keep them isolated from the text), providing an injection-safe SQL statement.

## Examples

**Query:**

```cs
using InterpolatedSql;
// ...

int categoryId = 10;
var query = new SqlBuilder($"SELECT * FROM Product WHERE CategoryId={categoryId}")
    .Build();

// query object now have these values:
// query.Sql == "SELECT * FROM Product WHERE CategoryId=@p0"
// query.Parameters["p0"].Value == 10
// You can just pass this object (with Sql and Parameters) to be used in your data access layer
```

**Command:**

```cs
using InterpolatedSql;
// ...

int categoryId = 10;
string productName = "Computer";
decimal price = 10.30;

var cmd = new SqlBuilder($"INSERT INTO Product (CategoryId, ProductName, Price) VALUES ({categoryId}, {productName}, {price})")
    .Build();

// cmd.Sql == "INSERT INTO Product (CategoryId, ProductName, Price) VALUES (@p0, @p1, @p2)"
// cmd.Parameters.Count == 3
// cmd.Parameters["p0"].Value == 10
// cmd.Parameters["p1"].Value == "Computer"
// cmd.Parameters["p2"].Value == 10.30;
```

**Multiline:**

```cs
using InterpolatedSql;
// ...

string productName = "%Laptop%";

var query = new SqlBuilder($@"
    SELECT * FROM Product
    WHERE
    CategoryID = {categoryId}
    AND Price <= {price}
    AND Name LIKE {productName}
    ORDER BY ProductId"
    ).Build();

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
This simple concept allows us to dynamically add new parameterized SQL clauses/conditions in a single C# statement:

```cs
string productName = "%Computer%";
int subCategoryId = 10;

var query = new SqlBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var cmd = query.Build();

// cmd.Sql == "SELECT * FROM Product WHERE 1==1 AND Name LIKE @p1 AND ProductSubcategoryID = @p2"
// cmd.Parameters["p0"].Value == "%Computer%""
// cmd.Parameters["p1"].Value == 10;
```

(If it wasn't for a single structure wrapping both the SQL text and the SQL Parameters, you would have to maintain and feed two independent objects - a StringBuilder and a parameters Dictionary).


# How to Use

**InterpolatedSql** library is **database-agnostic and ORM-agnostic**. It does NOT provide any database-specific code, and does NOT provide "ready to use" Data-Access methods. You can integrate the library with any existing data-layer based on ADO.NET.  

So it's mostly for developers that already have a data access layer and just want to easily build parametrized SQL statements. Or for developers who want to create their own extension-libraries (like InterpolatedSql.Dapper).

**If you want a "batteries-included" library you should use [InterpolatedSql.Dapper](./src/InterpolatedSql.Dapper/) library, which extends the different SQL Builders (explained below) to be used with Dapper micro-ORM.**


# SQL Builder Classes

This library provides different types of SQL builders for different purposes. 

InterpolatedSqlBuilderBase: This is an abstract class parent of all other builders, and contains most of the builders logic. It wraps the underlying StringBuilder (to store the SQL text) and the underlying parameters dictionary.
It contains methods for appending new blocks, and contains a bunch of other nice features. 
 
**SqlBuilder**: This is the most simple builder and can be used for any purpose. It just renders the statement that you explicitly append - it doesn't modify the generated statement.

**QueryBuilder**: This is a builder with some helpers to build queries (SELECT statements) with **dynamic filters**.
It provides methods like `.Where(filter)` which will automatically build a list of filters (WHERE conditions), and when the query is executed (or built) those filters are combined and inserted at the right place.
This builder can either append the `WHERE <filters>` automatically after the provided base-query, or if the query contains a filters **placeholder** then the filters will replace this placeholder.

**FluentQueryBuilder**: This is similar to **QueryBuilder** but it contains other methods to build step-by-step a syntatically-valid query. Differently from QueryBuilder, this one uses a different Fluent API that depending on the invoked method will return different interfaces - and those interfaces will only offer the methods that are valid in each stage. (e.g. initially it only accepts SELECTs, then it expects FROMs, then you can add some WHEREs, optionally some GROUP BY, HAVING, and finally ORDER BY).

To learn more about the different builders and their features, check the [Builders](./Builders.md) page.

To make advanced customizations using this library, check the [Advanced](./Advanced.md) page.

For other questions check our [FAQ](./FAQ.md) page.


# About

This library is a rewrite of the [DapperQueryBuilder](https://github.com/Drizin/DapperQueryBuilder) library. The purpose of this rewrite is to decouple the library from Dapper, make it more extensible, and leverage on InterpolatedStringHandlers (to avoid regex parsing). 

To see real-world usage examples, check-out the [InterpolatedSql.Dapper](./src/InterpolatedSql.Dapper/) library.

## Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/InterpolatedSql&type=Date)](https://star-history.com/#Drizin/InterpolatedSql&Date)


## License
MIT License


