﻿using NUnit.Framework;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder;
using InterpolatedSql.SqlBuilders;

namespace InterpolatedSql.Dapper.Tests
{
    public class FluentQueryBuilderTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            cn = new SqlConnection(TestHelper.GetMSSQLConnectionString());
        }
        #endregion

        string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId";

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }

        int maxPrice = 1000;
        int maxWeight = 15;
        string search = "%Mountain%";

        [Test]
        public void TestFluentAPI()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var q = cn.FluentQueryBuilder()
                .Select($"ProductId")
                .Select($"Name")
                .Select($"ListPrice")
                .Select($"Weight")
                .From($"[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where($"[Weight] <= {maxWeight}")
                .Where($"[Name] LIKE {search}")
                .OrderBy($"ProductId")
                .Build();

            Assert.AreEqual(expected, q.Sql);
            Assert.That(q.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p1"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p2"));
            Assert.AreEqual(q.DapperParameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(q.DapperParameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(q.DapperParameters.Get<string>("p2"), search);

            var products = q.Query<Product>();

            Assert.That(products.Any());
        }


        public class ProductCategories
        {
            public string Category { get; set; }
            public string Subcategory { get; set; }
            public string Name { get; set; }
            public string ProductNumber { get; set; }
        }

        [Test]
        public void JoinsTest()
        {
            var categories = new string[] { "Components", "Clothing", "Acessories" };
            var q = cn.FluentQueryBuilder()
                .SelectDistinct($"c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]")
                .From($"[Production].[Product] p")
                .From($"INNER JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
                .From($"INNER JOIN [Production].[ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]")
                .Where($"c.[Name] IN {categories}")
                .Build();
            var prods = q.Query<ProductCategories>();
        }

        [Test]
        public void FullQueryTest()
        {
            var q = cn.FluentQueryBuilder()
                .Select($"cat.[Name] as [Category]")
                .Select($"sc.[Name] as [Subcategory]")
                .Select($"AVG(p.[ListPrice]) as [AveragePrice]")
                .From($"[Production].[Product] p")
                .From($"LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
                .From($"LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]")
                .Where($"p.[ListPrice] BETWEEN {0} and {1000}")
                .Where($"cat.[Name] IS NOT NULL")
                .GroupBy($"cat.[Name]")
                .GroupBy($"sc.[Name]")
                .Having($"COUNT(*)>{5}")
                .Build();

            string expected =
@"SELECT cat.[Name] as [Category], sc.[Name] as [Subcategory], AVG(p.[ListPrice]) as [AveragePrice]
FROM [Production].[Product] p
LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]
WHERE p.[ListPrice] BETWEEN @p0 and @p1 AND cat.[Name] IS NOT NULL
GROUP BY cat.[Name], sc.[Name]
HAVING COUNT(*)>@p2";

            Assert.AreEqual(expected, q.Sql);

            var results = q.Query();

            Assert.That(results.Any());

        }


        [Test]
        public void TestAndOr()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND ([Weight] <= @p1 OR [Name] LIKE @p2)
ORDER BY ProductId";

            var q = cn.FluentQueryBuilder()
                .Select($"ProductId")
                .Select($"Name")
                .Select($"ListPrice")
                .Select($"Weight")
                .From($"[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where(new Filters(Filters.FiltersType.OR,
                    $"[Weight] <= {maxWeight}",
                    $"[Name] LIKE {search}"
                ))
                .OrderBy($"ProductId")
                .Build();

            Assert.AreEqual(expected, q.Sql);
            Assert.That(q.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p1"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p2"));
            Assert.AreEqual(q.DapperParameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(q.DapperParameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(q.DapperParameters.Get<string>("p2"), search);

            var products = q.Query<Product>();

            Assert.That(products.Any());
        }

        [Test]
        public void TestAndOr2()
        {
            int minPrice = 200;
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE ([ListPrice] >= @p0 AND [ListPrice] <= @p1) AND ([Weight] <= @p2 OR [Name] LIKE @p3)";

            var q = cn.FluentQueryBuilder()
                .Select($"ProductId, Name, ListPrice, Weight")
                .From($"[Production].[Product]")
                .Where(new Filters(
                    $"[ListPrice] >= {minPrice}",
                    $"[ListPrice] <= {maxPrice}"
                ))
                .Where(new Filters(Filters.FiltersType.OR,
                    $"[Weight] <= {maxWeight}",
                    $"[Name] LIKE {search}"
                ))
                .Build();

            Assert.AreEqual(expected, q.Sql);
            Assert.That(q.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p1"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p2"));
            Assert.That(q.DapperParameters.ParameterNames.Contains("p3"));
            Assert.AreEqual(q.DapperParameters.Get<int>("p0"), minPrice);
            Assert.AreEqual(q.DapperParameters.Get<int>("p1"), maxPrice);
            Assert.AreEqual(q.DapperParameters.Get<int>("p2"), maxWeight);
            Assert.AreEqual(q.DapperParameters.Get<string>("p3"), search);

            var products = q.Query<Product>();
        }

        [Test]
        public void TestDetachedFilters()
        {
            int minPrice = 200;
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var filters = new Filters(Filters.FiltersType.AND);
            filters.Add(new Filters()
            {
                new Filter($"[ListPrice] >= {minPrice}"),
                new Filter($"[ListPrice] <= {maxPrice}")
            });
            filters.Add(new Filters(Filters.FiltersType.OR)
            {
                new Filter($"[Weight] <= {maxWeight}"),
                new Filter($"[Name] LIKE {search}")
            });

            var whereClause = filters.Build();
            var parms = ParametersDictionary.LoadFrom(whereClause);
            // ParametersDictionary implements Dapper.SqlMapper.IDynamicParameters - so it can be passed directly to Dapper
            // But if you want to add to an existing Dapper.DynamicParameters you can do it:
            //foreach (var parameter in parms)
            //    SqlParameterMapper.Default.AddToDynamicParameters(dynamicParms, parameter.Value);

            Assert.AreEqual(@"WHERE ([ListPrice] >= @p0 AND [ListPrice] <= @p1) AND ([Weight] <= @p2 OR [Name] LIKE @p3)", whereClause.Sql);

            Assert.AreEqual(4, parms.ParameterNames.Count());
            Assert.AreEqual(minPrice, parms.Get<int>("p0"));
            Assert.AreEqual(maxPrice, parms.Get<int>("p1"));
            Assert.AreEqual(maxWeight, parms.Get<int>("p2"));
            Assert.AreEqual(search, parms.Get<string>("p3"));
        }


        [Test]
        public void GroupByOrderByQueryTest()
        {
            var q = cn.FluentQueryBuilder()
                .Select($"cat.[Name] as [Category]")
                .Select($"AVG(p.[ListPrice]) as [AveragePrice]")
                .From($"[Production].[Product] p")
                .From($"LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
                .From($"LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]")
                .Where($"p.[ListPrice] BETWEEN {0} and {1000}")
                .Where($"cat.[Name] IS NOT NULL")
                .GroupBy($"cat.[Name]")
                .Having($"COUNT(*)>{5}")
                .OrderBy($"cat.[Name]")
                .Build();

            string expected =
                @"SELECT cat.[Name] as [Category], AVG(p.[ListPrice]) as [AveragePrice]
FROM [Production].[Product] p
LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]
WHERE p.[ListPrice] BETWEEN @p0 and @p1 AND cat.[Name] IS NOT NULL
GROUP BY cat.[Name]
HAVING COUNT(*)>@p2
ORDER BY cat.[Name]";

            Assert.AreEqual(expected, q.Sql);

            var results = q.Query();

            Assert.That(results.Any());

        }

        [Test]
        public void GroupByWithNoWhereTest()
        {
            var q = cn.FluentQueryBuilder()
                .Select($"cat.[Name] as [Category]")
                .Select($"AVG(p.[ListPrice]) as [AveragePrice]")
                .From($"[Production].[Product] p")
                .From($"LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
                .From($"LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]")
                .GroupBy($"cat.[Name]")
                .Having($"COUNT(*)>{5}")
                .Build();

            string expected =
                @"SELECT cat.[Name] as [Category], AVG(p.[ListPrice]) as [AveragePrice]
FROM [Production].[Product] p
LEFT JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
LEFT JOIN [Production].[ProductCategory] cat on sc.[ProductCategoryID]=cat.[ProductCategoryID]
GROUP BY cat.[Name]
HAVING COUNT(*)>@p0";

            Assert.AreEqual(expected, q.Sql);

            var results = q.Query();

            Assert.That(results.Any());

        }

        [Test]
        public void FluentQueryBuilderInsideCommandBuilder()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";
            int customerId = 29825;

            var productsSubQuery = cn.FluentQueryBuilder()
                .Select($"ProductId")
                .From($"[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where($"[Weight] <= {maxWeight}")
                .Where($"[Name] LIKE {search}");

            short[] statuses = { 1, 2, 5 };
            var customerOrdersSubQuery = cn.FluentQueryBuilder()
                            .Select($"SalesOrderID")
                            .From($"[Sales].[SalesOrderHeader]")
                            .Where($"[CustomerId] = {customerId}")
                            .Where($"[Status] IN {statuses}");

            var finalQuery = cn
                .QueryBuilder($"SELECT * FROM [Sales].[SalesOrderDetail]")
                .Append($"WHERE [ProductId] IN ({productsSubQuery})")
                .Append($"AND [SalesOrderId] IN ({customerOrdersSubQuery})")
                .Build();

            string expected =
                @"SELECT * FROM [Sales].[SalesOrderDetail] WHERE [ProductId] IN (SELECT ProductId
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2) AND [SalesOrderId] IN (SELECT SalesOrderID
FROM [Sales].[SalesOrderHeader]
WHERE [CustomerId] = @p3 AND [Status] IN @parray4)";

            Assert.AreEqual(expected, finalQuery.Sql);
            Assert.That(finalQuery.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(finalQuery.DapperParameters.ParameterNames.Contains("p1"));
            Assert.That(finalQuery.DapperParameters.ParameterNames.Contains("p2"));
            Assert.That(finalQuery.DapperParameters.ParameterNames.Contains("p3"));
            Assert.That(finalQuery.DapperParameters.ParameterNames.Contains("parray4"));
            Assert.AreEqual(finalQuery.DapperParameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(finalQuery.DapperParameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(finalQuery.DapperParameters.Get<string>("p2"), search);
            Assert.AreEqual(finalQuery.DapperParameters.Get<int>("p3"), customerId);
            Assert.AreEqual(finalQuery.DapperParameters.Get<short[]>("parray4"), statuses);

            var orderItems = finalQuery.Query();

            Assert.That(orderItems.Any());
        }


    }
}
