using global::Dapper;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using InterpolatedSql.SqlBuilders;

namespace InterpolatedSql.Dapper.Tests
{
    public class QueryBuilderTests
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
ORDER BY ProductId
";

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }

        int maxPrice = 1000;
        int maxWeight = 15;
        string search = "%Mountain%";

        [Test]
        public void TestOperatorOverload()
        {
            string search = "%mountain%";
            var cmd = cn.QueryBuilder()
                + $@"SELECT * FROM [Production].[Product]"
                + $"WHERE [Name] LIKE {search}";
            cmd += $"AND 1=1";
            Assert.AreEqual("SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND 1=1", cmd.Build().Sql);
        }

        [Test]
        public void TestWhereIf()
        {
            var q = cn.QueryBuilder($"SELECT * FROM Products /**where**/");
            int? maxPrice = null;
            q.WhereIf(maxPrice != null, $"MaxPrice > {maxPrice}");

            var cmd = q.Build();
            Assert.AreEqual("SELECT * FROM Products /**where**/", cmd.Sql);
            Assert.That(cmd.DapperParameters.Count == 0);

            maxPrice = 100;
            q.WhereIf(maxPrice != null, $"MaxPrice > {maxPrice}");
            cmd = q.Build();
            Assert.AreEqual("SELECT * FROM Products WHERE MaxPrice > @p0", cmd.Sql);
            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p0"));
            Assert.AreEqual(cmd.DapperParameters.Get<int>("p0"), maxPrice);
        }

        [Test]
        public void TestTemplateAPI()
        {

            var q = cn.QueryBuilder(
$@"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
/**where**/
ORDER BY ProductId
");
            q.Where($"[ListPrice] <= {maxPrice}");
            q.Where($"[Weight] <= {maxWeight}");
            q.Where($"[Name] LIKE {search}");

            var cmd = q.Build();

            Assert.AreEqual(expected, cmd.Sql);
            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p1"));
            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p2"));
            Assert.AreEqual(cmd.DapperParameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(cmd.DapperParameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(cmd.DapperParameters.Get<string>("p2"), search);

            var products = q.Build().Query<Product>();

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
        public void TestQueryBuilderWithNestedFormattableString()
        {
            int orgId = 123;
            FormattableString innerQuery = $"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}";
            var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}").Build();

            Assert.AreEqual("SELECT FROM (SELECT Id, Name FROM SomeTable where OrganizationId=@p0) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p1", q.Sql);

            Assert.AreEqual(2, q.DapperParameters.Count);
            var p0 = q.DapperParameters["p0"];
            var p1 = q.DapperParameters["p1"];
            Assert.AreEqual(123, p0.Value);
            Assert.AreEqual(321, p1.Value);
        }

        [Test]
        public void TestQueryBuilderWithNestedFormattableString2()
        {
            int orgId = 123;
            FormattableString otherColumns = $"{"111111111"} AS {"ssn":raw}";
            FormattableString innerQuery = $"SELECT Id, Name, {otherColumns} FROM SomeTable where OrganizationId={orgId}";
            var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}").Build();

            Assert.AreEqual("SELECT FROM (SELECT Id, Name, @p0 AS ssn FROM SomeTable where OrganizationId=@p1) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p2", q.Sql);

            Assert.AreEqual(3, q.DapperParameters.Count);
            Assert.AreEqual("111111111", q.DapperParameters["p0"].Value);
            Assert.AreEqual(123, q.DapperParameters["p1"].Value);
            Assert.AreEqual(321, q.DapperParameters["p2"].Value);
        }

        [Test]
        public void TestQueryBuilderWithNestedFormattableString3()
        {
            string val1 = "val1";
            string val2 = "val2";
            FormattableString condition = $"col3 = {val2}";

            var q = cn.QueryBuilder($@"SELECT col1, {val1} as col2 FROM Table1 WHERE {condition}").Build();

            Assert.AreEqual("SELECT col1, @p0 as col2 FROM Table1 WHERE col3 = @p1", q.Sql);

            Assert.AreEqual(2, q.DapperParameters.Count);
            Assert.AreEqual(val1, q.DapperParameters["p0"].Value);
            Assert.AreEqual(val2, q.DapperParameters["p1"].Value);
        }

        [Test]
        public void TestQueryBuilderWithNestedQueryBuilder()
        {
            string val1 = "val1";
            string val2 = "val2";
            var condition = cn.SqlBuilder($"col3 = {val2}");

            var q = cn.QueryBuilder($@"SELECT col1, {val1} as col2 FROM Table1 WHERE {condition}").Build();

            Assert.AreEqual("SELECT col1, @p0 as col2 FROM Table1 WHERE col3 = @p1", q.Sql);

            Assert.AreEqual(2, q.DapperParameters.Count);
            Assert.AreEqual(val1, q.DapperParameters["p0"].Value);
            Assert.AreEqual(val2, q.DapperParameters["p1"].Value);
        }

        [Test]
        public void TestQueryBuilderFluentComposition()
        {

            var q = cn.QueryBuilder($"SELECT test FROM test")
                .Where($"test")
                .Append($"test") // Append on QueryBuilder still returns a QueryBuilder
                .Where($"test")
                ;
            Assert.AreEqual("SELECT test FROM test test WHERE test AND test", q.Build().Sql);
        }


    }
}
