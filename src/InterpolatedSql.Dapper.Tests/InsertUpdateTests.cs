using NUnit.Framework;
using System.Data;
using Microsoft.Data.SqlClient;
using InterpolatedSql.Dapper.SqlBuilders.InsertUpdateBuilder;

namespace InterpolatedSql.Dapper.Tests
{

    [TestFixture]
    public class InsertUpdateTests
    {
        IDbConnection cn;

        public InsertUpdateTests() { } // nunit requires parameterless constructor

        [SetUp]
        public void Setup()
        {
            cn = new SqlConnection(TestHelper.GetMSSQLConnectionString());
        }

        [Test]
        public void TestInsert()
        {
            var insert = cn
                .InsertUpdateBuilder("[Production].[Product]")
                .AddColumn("ProductID", 1, includeInInsert: false, includeInUpdate: false)
                .AddColumn("Name", "My Product")
                .AddColumn("ProductNumber", "AR-5381")
                .AddColumn("ListPrice", 10.20)
                .GetInsertSql();

            Assert.AreEqual("INSERT INTO [Production].[Product] (Name, ProductNumber, ListPrice) VALUES (@p0, @p1, @p2);", insert.Sql);
            Assert.AreEqual(3, insert.SqlParameters.Count);
            Assert.AreEqual("My Product", insert.DapperParameters["p0"].Value);
            Assert.AreEqual("AR-5381", insert.DapperParameters["p1"].Value);
            Assert.AreEqual(10.20, insert.DapperParameters["p2"].Value);
        }


        [Test]
        public void TestUpdate()
        {
            int productId = 1;

            var update = cn
                .InsertUpdateBuilder("[Production].[Product]")
                .AddColumn("ProductID", 1, includeInInsert: false, includeInUpdate: false)
                .AddColumn("Name", "My Product")
                .AddColumn("ProductNumber", "AR-5381")
                .AddColumn("ListPrice", 10.20)
                .GetUpdateSql($"ProductID={productId}");

            Assert.AreEqual("UPDATE [Production].[Product] SET Name=@p0, ProductNumber=@p1, ListPrice=@p2 WHERE ProductID=@p3", update.Sql);
            Assert.AreEqual(4, update.SqlParameters.Count);
            Assert.AreEqual("My Product", update.DapperParameters["p0"].Value);
            Assert.AreEqual("AR-5381", update.DapperParameters["p1"].Value);
            Assert.AreEqual(10.20, update.DapperParameters["p2"].Value);
            Assert.AreEqual(productId, update.DapperParameters["p3"].Value);
        }

    }
}
