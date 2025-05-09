using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using InterpolatedSql;
using InterpolatedSql.SqlBuilders;
using System.Threading.Tasks;
using InterpolatedSql.Tests;
using Dapper;
//using TestHelper = InterpolatedSql.Dapper.Tests.TestHelper;

namespace InterpolatedSql.Dapper.Tests
{

    [TestFixture(true)]
    [TestFixture(false)]

    public class SqlBuilderTests
    {
        IDbConnection cn;

        public SqlBuilderTests() { } // nunit requires parameterless constructor
        public SqlBuilderTests(bool reuseIdenticalParameters)
        {
            InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters = reuseIdenticalParameters;
        }

        #region Setup
        [SetUp]
        public void Setup()
        {
            cn = new SqlConnection(TestHelper.GetMSSQLConnectionString());
        }
        #endregion

        int businessEntityID = 1;
        string nationalIDNumber = "295847284";
        DateTime birthDate = new DateTime(1969, 01, 29);
        string maritalStatus = "S"; // single
        string gender = "M";

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }

        public class SalesOrderHeader
        {
            public int SalesOrderID { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? ShipDate { get; set; }
        }
        public class SalesOrderDetail
        {
            short OrderQty { get; set; }
            decimal UnitPrice { get; set; }
        }

        public class SalesOrderViewModel
        {
            public int SalesOrderID { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? ShipDate { get; set; }
            public List<SalesOrderDetailViewModel> OrderItems { get; set; }
        }
        public class SalesOrderDetailViewModel
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            short OrderQty { get; set; }
            decimal UnitPrice { get; set; }
        }

        [Test]
        public void TestBareCommand()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;

            var query = cn
                .SqlBuilder($$"""
                SELECT * FROM [Production].[Product]
                WHERE
                [Name] LIKE {{productName}}
                AND [ProductSubcategoryID] = {{subCategoryId}}
                ORDER BY [ProductId]
                """);

            Assert.AreEqual(@"
SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]".TrimStart(), query.Build().Sql);

            var products = query.Query<Product>();
        }


        [Test]
        public void TestNameof()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;

            var query = cn
                .SqlBuilder($$"""
                SELECT * FROM [Production].[Product]
                WHERE
                [{{nameof(Product.Name):raw}}] LIKE {{productName}}
                AND [ProductSubcategoryID] = {{subCategoryId}}
                ORDER BY [ProductId]
                """);

            Assert.AreEqual(@"
SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]".TrimStart(), query.Build().Sql);

            var products = query.Query<Product>();
        }

        [Test]
        public void TestAppends()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;

            var query = cn
                .SqlBuilder($@"SELECT * FROM [Production].[Product]")
                .AppendLine($"WHERE")
                .AppendLine($"[Name] LIKE {productName}")
                .AppendLine($"AND [ProductSubcategoryID] = {subCategoryId}")
                .AppendLine($"ORDER BY [ProductId]");
            Assert.AreEqual(
@"SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]", query.Build().Sql);

            var products = query.Query<Product>();
        }

        [Test]
        public void TestAutoSpacing()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;
#if NET6_0_OR_GREATER
            var query2 = cn
                .SqlBuilder($@"SELECT * FROM [Production].[Product]")
                .Append($"WHERE");
#endif
            var query = cn
                .SqlBuilder($@"SELECT * FROM [Production].[Product]")
                .Append($"WHERE")
                .Append($"[Name] LIKE {productName}")
                .Append($"AND [ProductSubcategoryID] = {subCategoryId}")
                .Append($"ORDER BY [ProductId]");
            
            Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND [ProductSubcategoryID] = @p1 ORDER BY [ProductId]", query.Build().Sql);

            var products = query.Query<Product>();
        }

        [Test]
        public void TestStoredProcedure()
        {
            var q = cn.SqlBuilder($"[HumanResources].[uspUpdateEmployeePersonalInfo]")
                .AddParameter("BusinessEntityID", businessEntityID)
                .AddParameter("NationalIDNumber", nationalIDNumber)
                .AddParameter("BirthDate", birthDate)
                .AddParameter("MaritalStatus", maritalStatus)
                .AddParameter("Gender", gender);
            int affected = q.Execute(commandType: CommandType.StoredProcedure);
        }

        [Test]
        public async Task TestStoredProcedureAsync()
        {
            var q = await cn.SqlBuilder($"[HumanResources].[uspUpdateEmployeePersonalInfo]")
                .AddParameter("BusinessEntityID", businessEntityID)
                .AddParameter("NationalIDNumber", nationalIDNumber)
                .AddParameter("BirthDate", birthDate)
                .AddParameter("MaritalStatus", maritalStatus)
                .AddParameter("Gender", gender)
                .ExecuteAsync(commandType: CommandType.StoredProcedure);
        }


        [Test]
        public void TestStoredProcedureExec()
        {
            var q = cn.SqlBuilder($@"
                DECLARE @ret INT;
                EXEC @RET = [HumanResources].[uspUpdateEmployeePersonalInfo] 
                   @BusinessEntityID={businessEntityID}
                  ,@NationalIDNumber={nationalIDNumber}
                  ,@BirthDate={birthDate}
                  ,@MaritalStatus={maritalStatus}
                  ,@Gender={gender};
                SELECT @RET;
                ");

            int affected = q.Execute();
        }

        public class MyPoco { public int MyValue { get; set; } }

        [Test]
        public void TestStoredProcedureOutput()
        {
            MyPoco poco = new MyPoco();

            var cmd = cn.SqlBuilder($"[dbo].[sp_TestOutput]")
                .AddParameter("Input1", dbType: DbType.Int32);
            //.AddParameter("Output1",  dbType: DbType.Int32, direction: ParameterDirection.Output);
            //var getter = ParameterInfos.GetSetter((MyPoco p) => p.MyValue);
            var outputParm = new DbTypeParameterInfo("Output1", size: 4);
            outputParm.ConfigureOutputParameter(poco, p => p.MyValue, SqlParameterInfo.OutputParameterDirection.Output);
            cmd.AddParameter(outputParm); //TODO: AddOutputParameter? move ConfigureOutputParameter inside it. // previously this was cmd.Parameters.Add, but now Parameters is get-only
            int affected = cmd.Execute(commandType: CommandType.StoredProcedure);

            string outputValue = cmd.Build().DapperParameters.Get<string>("Output1"); // why is this being returned as string? just because I didn't provide type above?
            Assert.AreEqual(outputValue, "2");

            Assert.AreEqual(poco.MyValue, 2);
        }

        [Test]
        public void TestCRUD()
        {
            string id = "123";
            int affected = cn.SqlBuilder($@"UPDATE [HumanResources].[Employee] SET")
                .Append($"NationalIDNumber={id}")
                .Append($"WHERE BusinessEntityID={businessEntityID}")
                .Execute();
        }

        /// <summary>
        /// Quotes around interpolated arguments should be automtaically detected and ignored
        /// </summary>
        [Test]
        public void TestQuotes1()
        {
            string search = "%mountain%";
            string expected = "SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0";
            var cmd = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {search}");
            var cmd2 = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE '{search}'");
            
            Assert.AreEqual(expected, cmd.Build().Sql);
            Assert.AreEqual(expected, cmd2.Build().Sql);

            var products = cmd.Query<Product>();
            Assert.That(products.Any());
            var products2 = cmd2.Query<Product>();
            Assert.That(products2.Any());
        }


        /// <summary>
        /// Quotes around interpolated arguments should be automtaically detected and ignored
        /// </summary>
        [Test]
        public void TestQuotes2()
        {
            string productNumber = "AR-5381";
            string expected = "SELECT * FROM [Production].[Product] WHERE [ProductNumber]=@p0";
            var cmd = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [ProductNumber]='{productNumber}'");
            var cmd2 = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [ProductNumber]={productNumber}");

            Assert.AreEqual(expected, cmd.Build().Sql);
            Assert.AreEqual(expected, cmd2.Build().Sql);

            var products = cmd.Query<Product>();
            Assert.That(products.Any());
            var products2 = cmd2.Query<Product>();
            Assert.That(products2.Any());
        }


        /// <summary>
        /// Quotes around interpolated arguments should be automtaically detected and ignored
        /// </summary>
        [Test]
        public void TestQuotes3()
        {
            string productNumber = "AR-5381";
            string expected = "SELECT * FROM [Production].[Product] WHERE @p0<=[ProductNumber]";
            var cmd = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE '{productNumber}'<=[ProductNumber]");
            var cmd2 = cn.SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE {productNumber}<=[ProductNumber]");

            Assert.AreEqual(expected, cmd.Build().Sql);
            Assert.AreEqual(expected, cmd2.Build().Sql);

            var products = cmd.Query<Product>();
            Assert.That(products.Any());
            var products2 = cmd2.Query<Product>();
            Assert.That(products2.Any());
        }


        /// <summary>
        /// Quotes around interpolated arguments should not be ignored if it's raw string
        /// </summary>
        [Test]
        public void TestQuotes4()
        {
            string literal = "Hello";
            string search = "%mountain%";

            string expected = "SELECT 'Hello' FROM [Production].[Product] WHERE [Name] LIKE @p0";
            var cmd = cn.SqlBuilder($@"SELECT '{literal:raw}' FROM [Production].[Product] WHERE [Name] LIKE {search}"); // quotes will be preserved

            string expected2 = "SELECT @p0 FROM [Production].[Product] WHERE [Name] LIKE @p1";
            var cmd2 = cn.SqlBuilder($@"SELECT '{literal}' FROM [Production].[Product] WHERE [Name] LIKE {search}"); // quotes will be striped

            Assert.AreEqual(expected, cmd.Build().Sql);
            Assert.AreEqual(expected2, cmd2.Build().Sql);

            var products = cmd.Query<Product>();
            Assert.That(products.Any());

            var products2 = cmd2.Query<Product>();
            Assert.That(products2.Any());
        }


        [Test]
        public void TestAutospacing()
        {
            string search = "%mountain%";
            var cmd = cn.SqlBuilder($@"SELECT * FROM [Production].[Product]");
            cmd.Append($"WHERE [Name] LIKE {search}");
            cmd.Append($"AND 1=1");
            Assert.AreEqual("SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND 1=1", cmd.Build().Sql);
        }

        [Test]
        public void TestOperatorOverload()
        {
            string search = "%mountain%";
            var cmd = cn.SqlBuilder()
                + $@"SELECT * FROM [Production].[Product]"
                + $"WHERE [Name] LIKE {search}";
            cmd += $"AND 1=1";
            Assert.AreEqual("SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND 1=1", cmd.Build().Sql);
        }

        [Test]
        public void TestAutospacing2()
        {
            string search = "%mountain%";
            var cmd = cn.SqlBuilder($$"""
                            SELECT * FROM [Production].[Product]
                            WHERE [Name] LIKE {{search}}
                            AND 1=2
                            """);
            Assert.AreEqual(
                "SELECT * FROM [Production].[Product]" + Environment.NewLine + 
                "WHERE [Name] LIKE @p0" + Environment.NewLine + 
                "AND 1=2", cmd.Build().Sql);
        }

        [Test]
        public void TestAutospacing3()
        {
            string productNumber = "EC-M092";
            int productId = 328;
            var cmd = cn.SqlBuilder($$"""
                UPDATE [Production].[Product]
                SET [ProductNumber]={{productNumber}}
                WHERE [ProductId]={{productId}}
                """);

            string expected = 
                "UPDATE [Production].[Product]" + Environment.NewLine + 
                "SET [ProductNumber]=@p0" + Environment.NewLine +
                "WHERE [ProductId]=@p1";
            
            Assert.AreEqual(expected, cmd.Build().Sql);
        }

        [Test]
        public void TestAutospacing4()
        {
            string productNumber = "EC-M092";
            int productId = 328;

            var cmd = cn.SqlBuilder($@"UPDATE [Production].[Product]")
                .Append($"SET [ProductNumber]={productNumber}")
                .Append($"WHERE [ProductId]={productId}");

            string expected = "UPDATE [Production].[Product] SET [ProductNumber]=@p0 WHERE [ProductId]=@p1";

            Assert.AreEqual(expected, cmd.Build().Sql);
        }


        [Test]
        public void TestQueryBuilderWithAppends()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;

            var query = cn
                .SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {productName}");
            query.AppendLine($"AND [ProductSubcategoryID] = {subCategoryId} ORDER BY {2}");
            Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0
AND [ProductSubcategoryID] = @p1 ORDER BY @p2", query.Build().Sql);

            //var products = query.Query<Product>();
        }

        [Test]
        public void TestQueryBuilderWithAppends2()
        {
            string productName = "%mountain%";
            int subCategoryId = 12;

            var query = cn
                .SqlBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {productName}");
            query.AppendLine($"AND [ProductSubcategoryID]={subCategoryId} ORDER BY {2}");
            Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0
AND [ProductSubcategoryID]=@p1 ORDER BY @p2", query.Build().Sql);

            //var products = query.Query<Product>();
        }


        [Test]
        public void TestRepeatedParameters()
        {
            string username = "rdrizin";
            int subCategoryId = 12;
            int? categoryId = null;

            var query = cn.SqlBuilder($@"SELECT * FROM [table1] WHERE ([Name]={username} or [Author]={username}");
            query.Append($"or [Creator]={username})");
            query.Append($"AND ([ProductSubcategoryID]={subCategoryId}");
            query.Append($"OR [ProductSubcategoryID]={categoryId}");
            query.Append($"OR [ProductCategoryID]={subCategoryId}");
            query.Append($"OR [ProductCategoryID]={categoryId})");

            if (query.Options.ReuseIdenticalParameters)
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE ([Name]=@p0 or [Author]=@p0"
                    + " or [Creator]=@p0)"
                    + " AND ([ProductSubcategoryID]=@p1"
                    + " OR [ProductSubcategoryID]=@p2"
                    + " OR [ProductCategoryID]=@p1"
                    + " OR [ProductCategoryID]=@p2)"
                    , query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p1"), 12);
                Assert.AreEqual(query.Build().DapperParameters.Get<int?>("p2"), null);
            }
            else
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE ([Name]=@p0 or [Author]=@p1"
                    + " or [Creator]=@p2)"
                    + " AND ([ProductSubcategoryID]=@p3"
                    + " OR [ProductSubcategoryID]=@p4"
                    + " OR [ProductCategoryID]=@p5"
                    + " OR [ProductCategoryID]=@p6)"
                    , query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p3"), 12);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p5"), 12);
                Assert.AreEqual(query.Build().DapperParameters.Get<int?>("p4"), null);
                Assert.AreEqual(query.Build().DapperParameters.Get<int?>("p6"), null);
            }
        }


        [Test]
        public void TestRepeatedParameters2()
        {
            if (!InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters)
                return;

            int? fileId = null;
            string backupFileName = null;
            var folderKey = 93572;
            var secureFileName = "upload.txt";
            var uploadDate = DateTime.Now;
            string description = null;
            int? size = null;
            var cacheId = new Guid("{95b94695-b7ec-4e3a-9260-f815cceb5ff1}");
            var version = new Guid("{95b94695-b7ec-4e3a-9260-f815cceb5ff1}");
            var user = "terry.aney";
            var contentType = "application/pdf";
            var folder = "btr.aney.terry";

            var query = cn.SqlBuilder($@"DECLARE @fKey int; SET @fKey = {fileId};
DECLARE @backupFileName VARCHAR(1); SET @backupFileName = {backupFileName};
IF @backupFileName IS NOT NULL BEGIN
    UPDATE [File] SET Name = @backupFileName WHERE UID = @fKey;
    SET @fKey = NULL;
END
IF @fKey IS NULL BEGIN
    INSERT INTO [File] ( Folder, Name, CreateTime, Description )
    VALUES ( {folderKey}, {secureFileName}, {uploadDate}, {description} )
    SELECT @fKey = SCOPE_IDENTITY();
END ELSE BEGIN
    -- File Existed
    UPDATE [File] SET Deleted = 0, Description = ISNULL({description}, Description) WHERE UID = @fKey
END
DECLARE @size int; SET @size = {size};
-- File was compressed during upload so the 'original' file size is wrong and need to query the length of the content
IF @size IS NULL BEGIN
    SELECT @size = DATALENGTH( Content )
    FROM Cache
    WHERE UID = {cacheId}
END
INSERT INTO Version ( [File], VersionID, Time, UploadedBy, ContentType, Size, VersionIndex, DataLockerToken )
VALUES ( @fKey, {version}, {uploadDate}, {user}, {contentType}, @size, 0, {cacheId} )
INSERT INTO [Log] ( Action, FolderName, FileName, VersionId, VersionIndex, [User], Size, Time )
VALUES ( 'I', {folder}, {secureFileName}, {version}, 0, {user}, @size, {uploadDate} )
SELECT @fKey");

            Assert.AreEqual(@"DECLARE @fKey int; SET @fKey = @p0;
DECLARE @backupFileName VARCHAR(1); SET @backupFileName = @p0;
IF @backupFileName IS NOT NULL BEGIN
    UPDATE [File] SET Name = @backupFileName WHERE UID = @fKey;
    SET @fKey = NULL;
END
IF @fKey IS NULL BEGIN
    INSERT INTO [File] ( Folder, Name, CreateTime, Description )
    VALUES ( @p1, @p2, @p3, @p0 )
    SELECT @fKey = SCOPE_IDENTITY();
END ELSE BEGIN
    -- File Existed
    UPDATE [File] SET Deleted = 0, Description = ISNULL(@p0, Description) WHERE UID = @fKey
END
DECLARE @size int; SET @size = @p0;
-- File was compressed during upload so the 'original' file size is wrong and need to query the length of the content
IF @size IS NULL BEGIN
    SELECT @size = DATALENGTH( Content )
    FROM Cache
    WHERE UID = @p4
END
INSERT INTO Version ( [File], VersionID, Time, UploadedBy, ContentType, Size, VersionIndex, DataLockerToken )
VALUES ( @fKey, @p4, @p3, @p5, @p6, @size, 0, @p4 )
INSERT INTO [Log] ( Action, FolderName, FileName, VersionId, VersionIndex, [User], Size, Time )
VALUES ( 'I', @p7, @p2, @p4, 0, @p5, @size, @p3 )
SELECT @fKey", query.Build().Sql);

            Assert.AreEqual(query.Build().DapperParameters.Get<int?>("p0"), null);
            Assert.AreEqual(query.Build().DapperParameters.Get<int>("p1"), folderKey);
            Assert.AreEqual(query.Build().DapperParameters.Get<string>("p2"), secureFileName);
            Assert.AreEqual(query.Build().DapperParameters.Get<DateTime>("p3"), uploadDate);
            Assert.AreEqual(query.Build().DapperParameters.Get<Guid>("p4"), cacheId);
            Assert.AreEqual(query.Build().DapperParameters.Get<string>("p5"), user);
            Assert.AreEqual(query.Build().DapperParameters.Get<string>("p6"), contentType);
            Assert.AreEqual(query.Build().DapperParameters.Get<string>("p7"), folder);
        }

        [Test]
        public void TestRepeatedParameters3() // without leading spaces
        {
            var cn = new SqlConnection();
            var qb = cn.SqlBuilder($"{"A"}");
            qb.Append($"{"B"}");
            Assert.AreEqual("@p0 @p1", qb.Build().Sql);
        }

        [Test]
        public void TestRepeatedParameters4()
        {
            var cn = new SqlConnection();
            var qb = cn.SqlBuilder();
            qb.Append($"{"A"},{"B"},{"C"},{"D"},{"E"},{"F"},{"G"},{"H"},{"I"},{"J"},{"K"},"); // @p0-@p10
            qb.Append($"{1},{2},{3},{4},{4},{5},{6},{7},{8},{9},{10},"); // @p10-@p20, with repeated @p14

            qb.Append($"{"A"}"); // @p21 should reuse @p0
            qb.Append($"{"B"}"); // @p22 should reuse @p1

            if (qb.Options.ReuseIdenticalParameters)
                Assert.AreEqual("@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p0 @p1", qb.Build().Sql);
            else
                Assert.AreEqual("@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22 @p23", qb.Build().Sql);
        }
        
        [Test]
        public void TestRepeatedParameters5()
        {
            var cn = new SqlConnection();
            var qb = cn.SqlBuilder();
            qb.Append($"{"A"}"); // @p0
            qb.Append($"{"B"}"); // @p1

            qb.Append($"{2}"); // @p2
            qb.Append($"{3}"); // @p3
            qb.Append($"{4}"); // @p4
            qb.Append($"{5}"); // @p5
            qb.Append($"{6}"); // @p6
            qb.Append($"{7}"); // @p7
            qb.Append($"{8}"); // @p8
            qb.Append($"{9}"); // @p9

            qb.Append($"{10}"); // @p10
            qb.Append($"{11}"); // @p11
            qb.Append($"{12}"); // @p12
            qb.Append($"{13}"); // @p13
            qb.Append($"{14}"); // @p14
            qb.Append($"{15}"); // @p15
            qb.Append($"{16}"); // @p16
            qb.Append($"{17}"); // @p17
            qb.Append($"{18}"); // @p18
            qb.Append($"{19}"); // @p19

            qb.Append($"{"A"}"); // @p20 should reuse @p0
            qb.Append($"{"B"}"); // @p21 should reuse @p1

            if (qb.Options.ReuseIdenticalParameters)
                Assert.AreEqual("@p0 @p1 @p2 @p3 @p4 @p5 @p6 @p7 @p8 @p9 @p10 @p11 @p12 @p13 @p14 @p15 @p16 @p17 @p18 @p19 @p0 @p1", qb.Build().Sql);
            else
                Assert.AreEqual("@p0 @p1 @p2 @p3 @p4 @p5 @p6 @p7 @p8 @p9 @p10 @p11 @p12 @p13 @p14 @p15 @p16 @p17 @p18 @p19 @p20 @p21", qb.Build().Sql);
        }

        [TestCase(null, null)]
        [TestCase(2, null)]
        [TestCase(null, 1)]
        [TestCase(2, 1)]
        public void TestAppendIf(int? categoryId, int? subCategoryId)
        {
            var query = cn.SqlBuilder($@"SELECT * FROM [table1] WHERE 1=1");
            query.AppendIf(categoryId != null, $"AND [ProductCategoryID]={categoryId}");
            query.AppendIf(subCategoryId != null, $"AND [ProductSubcategoryID]={subCategoryId}");

            if (categoryId == null && subCategoryId == null)
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE 1=1", query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Count, 0);
            }
            if (categoryId != null && subCategoryId == null)
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE 1=1 AND [ProductCategoryID]=@p0", query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Count, 1);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p0"), 2);
            }
            if (categoryId == null && subCategoryId != null)
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE 1=1 AND [ProductSubcategoryID]=@p0", query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Count, 1);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p0"), 1);
            }
            if (categoryId != null && subCategoryId != null)
            {
                Assert.AreEqual(@"SELECT * FROM [table1] WHERE 1=1 AND [ProductCategoryID]=@p0 AND [ProductSubcategoryID]=@p1", query.Build().Sql);
                Assert.AreEqual(query.Build().DapperParameters.Count, 2);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p0"), 2);
                Assert.AreEqual(query.Build().DapperParameters.Get<int>("p1"), 1);
            }
            //TODO: AppendIf with Func<FormattableString> for cases like using LINQ/Arrays without having to test edge cases
        }



        [Test]
        public void TestMultipleStatements()
        {
            int orderId = 10;
            string currentUserId = "admin";

            bool softDelete = true;
            string action = "DELETED_ORDER";
            string description = $"User {currentUserId} deleted order {orderId}";

            var cmd = cn.SqlBuilder();
            if (softDelete)
                cmd.Append($"UPDATE Orders SET IsDeleted=1 WHERE OrderId = {orderId}; ");
            else
                cmd.Append($"DELETE FROM Orders WHERE OrderId = {orderId}; ");
            cmd.Append($"INSERT INTO Logs (Action, UserId, Description) VALUES ({action}, {orderId}, {description}); ");

            if (cmd.Options.ReuseIdenticalParameters)
            {
                Assert.AreEqual(cmd.Build().DapperParameters.Count, 3);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<int>("p0"), orderId);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<string>("p1"), action);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<string>("p2"), description);
            }
            else
            {
                Assert.AreEqual(cmd.Build().DapperParameters.Count, 4);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<int>("p0"), orderId);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<string>("p1"), action);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<int>("p2"), orderId);
                Assert.AreEqual(cmd.Build().DapperParameters.Get<string>("p3"), description);
            }
        }

        [Test]
        public void TestQueryBuilderWithJoins()
        {
            string productName = "%mountain%";
            string joinParam = "test";

            var query = cn
                .QueryBuilder($@"SELECT * FROM [Table1] /**joins**/ WHERE [Table1].[Name] LIKE {productName}");

            query.From($"INNER JOIN [Table2] on [Table1].Table2Id=[Table2].Id and [Table2].Name={joinParam}");

            Assert.AreEqual("SELECT * FROM [Table1] INNER JOIN [Table2] on [Table1].Table2Id=[Table2].Id and [Table2].Name=@p1 WHERE [Table1].[Name] LIKE @p0", query.Build().Sql);
        }

        [Test]
        public void TestQueryBuilderWithFrom()
        {
            string productName = "%mountain%";
            string joinParam = "test";

            var query = cn
                .QueryBuilder($@"SELECT * /**from**/ WHERE [Table1].[Name] LIKE {productName}");

            query.From($"[Table1]")
                .From($"INNER JOIN [Table2] on [Table1].Table2Id=[Table2].Id and [Table2].Name={joinParam}");

            Assert.AreEqual(@"SELECT * FROM [Table1]
INNER JOIN [Table2] on [Table1].Table2Id=[Table2].Id and [Table2].Name=@p1 WHERE [Table1].[Name] LIKE @p0", query.Build().Sql);
        }

        [Test]
        public void TestQueryBuilderWithSelect()
        {
            var query = cn.QueryBuilder(
$@"SELECT 
    *
    /**selects**/ 
FROM 
    [Table1]");

            query.Select($"'Test' as AnotherColumn").Select($"'Test2' as AnotherColumn2");

            Assert.AreEqual(
@"SELECT 
    *
    , 'Test' as AnotherColumn, 'Test2' as AnotherColumn2 
FROM 
    [Table1]", query.Build().Sql);
        }

        [Test]
        public void MultiMappingTest()
        {
            int orderId = 43659;
            var query = cn.SqlBuilder($@"
                SELECT
                    o.*, NULL as [ColSplitter1],
                    d.*, NULL as [ColSplitter2],
                    p.*
                FROM 
                    [Sales].[SalesOrderHeader] o INNER JOIN 
                    [Sales].[SalesOrderDetail] d ON o.SalesOrderID=d.SalesOrderID INNER JOIN
                    [Production].[Product] p ON d.ProductID = p.ProductID
                WHERE o.SalesOrderID={orderId}
            ");
            Dictionary<int, SalesOrderViewModel> ordersDict = new();
            var joinedOrders = query.Query<SalesOrderHeader, SalesOrderDetail, Product, SalesOrderViewModel>(
                (o, d, p) => 
                {
                    SalesOrderViewModel order;
                    if (!ordersDict.TryGetValue(o.SalesOrderID, out order))
                    {
                        ordersDict[o.SalesOrderID] = new SalesOrderViewModel()
                        {
                            SalesOrderID = o.SalesOrderID,
                            OrderDate = o.OrderDate,
                            ShipDate = o.ShipDate,
                            OrderItems = new()
                        };
                    }
                    ordersDict[o.SalesOrderID].OrderItems.Add(new SalesOrderDetailViewModel()
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                    });
                    return ordersDict[o.SalesOrderID];
                },
                splitOn: "ColSplitter1, ColSplitter2"
                );
#if NET6_0_OR_GREATER
            var orders = joinedOrders.DistinctBy(o => o.SalesOrderID);
#else
            Assert.AreEqual(1, joinedOrders.Select(o => o.OrderItems).Distinct().Count());
            var orders = joinedOrders.Take(1);
#endif
            Assert.AreEqual(1, orders.Select(o => o.OrderItems).Distinct().Count());
            Assert.AreEqual(orderId, orders.Single().SalesOrderID);
            Assert.AreEqual(12, orders.Single().OrderItems.Count());
            Assert.That(orders.Single().OrderItems.Where(i => i.ProductId == 776).Count()==1);
            Assert.That(orders.Single().OrderItems.Where(i => i.ProductId == 777).Count() == 1);
        }

        [Test]
        public void ArrayTest()
        {
            //https://github.com/Drizin/DapperQueryBuilder/issues/22
            string v = "a";
            List<int> numList = new List<int> { 1, 2, 3, 4, 5, 6 };

            FormattableString script = $@"
declare @v1 nvarchar(10)={v}
declare @v2 nvarchar(10)={v}
select 1 from tb where name in {numList}
declare @v3 nvarchar(10)={v}
declare @v4 nvarchar(10)={v}
declare @v5 nvarchar(10)={v}
declare @v6 nvarchar(10)={v}
declare @v7 nvarchar(10)={v}
declare @v8 nvarchar(10)={v}
declare @v9 nvarchar(10)={v}
declare @v10 nvarchar(10)={v}
declare @v11 nvarchar(10)={v}
declare @v12 nvarchar(10)={v}
declare @v13 nvarchar(10)={v}
declare @v14 nvarchar(10)={v}
declare @v15 nvarchar(10)={v}
declare @v16 nvarchar(10)={v}
declare @v17 nvarchar(10)={v}
declare @v18 nvarchar(10)={v}
declare @v19 nvarchar(10)={v}
declare @v20 nvarchar(10)={v}
declare @v21 nvarchar(10)={v}
declare @v22 nvarchar(10)={v}
declare @v23 nvarchar(10)={v}
select 'ok'
";

            var query = cn.QueryBuilder(script).Build();
            var s = query.Sql;
            var p = query.DapperParameters;

            if (InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters)
            {
                Assert.AreEqual(@"
declare @v1 nvarchar(10)=@p0
declare @v2 nvarchar(10)=@p0
select 1 from tb where name in @p1array
declare @v3 nvarchar(10)=@p0
declare @v4 nvarchar(10)=@p0
declare @v5 nvarchar(10)=@p0
declare @v6 nvarchar(10)=@p0
declare @v7 nvarchar(10)=@p0
declare @v8 nvarchar(10)=@p0
declare @v9 nvarchar(10)=@p0
declare @v10 nvarchar(10)=@p0
declare @v11 nvarchar(10)=@p0
declare @v12 nvarchar(10)=@p0
declare @v13 nvarchar(10)=@p0
declare @v14 nvarchar(10)=@p0
declare @v15 nvarchar(10)=@p0
declare @v16 nvarchar(10)=@p0
declare @v17 nvarchar(10)=@p0
declare @v18 nvarchar(10)=@p0
declare @v19 nvarchar(10)=@p0
declare @v20 nvarchar(10)=@p0
declare @v21 nvarchar(10)=@p0
declare @v22 nvarchar(10)=@p0
declare @v23 nvarchar(10)=@p0
select 'ok'
", query.Sql);

                Assert.AreEqual(query.DapperParameters.Get<string>("p0"), v);
                Assert.AreEqual(query.DapperParameters.Get<List<int>>("p1array"), numList);
            }
            else
            {
                Assert.AreEqual(@"
declare @v1 nvarchar(10)=@p0
declare @v2 nvarchar(10)=@p1
select 1 from tb where name in @p2array
declare @v3 nvarchar(10)=@p3
declare @v4 nvarchar(10)=@p4
declare @v5 nvarchar(10)=@p5
declare @v6 nvarchar(10)=@p6
declare @v7 nvarchar(10)=@p7
declare @v8 nvarchar(10)=@p8
declare @v9 nvarchar(10)=@p9
declare @v10 nvarchar(10)=@p10
declare @v11 nvarchar(10)=@p11
declare @v12 nvarchar(10)=@p12
declare @v13 nvarchar(10)=@p13
declare @v14 nvarchar(10)=@p14
declare @v15 nvarchar(10)=@p15
declare @v16 nvarchar(10)=@p16
declare @v17 nvarchar(10)=@p17
declare @v18 nvarchar(10)=@p18
declare @v19 nvarchar(10)=@p19
declare @v20 nvarchar(10)=@p20
declare @v21 nvarchar(10)=@p21
declare @v22 nvarchar(10)=@p22
declare @v23 nvarchar(10)=@p23
select 'ok'
", query.Sql);

                Assert.AreEqual(query.DapperParameters.Get<string>("p0"), v);
                Assert.AreEqual(query.DapperParameters.Get<string>("p1"), v);
                Assert.AreEqual(query.DapperParameters.Get<List<int>>("p2array"), numList);
            }
        }

        [Test]
        public void DapperCollisionTest1()
        {
            //https://github.com/Drizin/InterpolatedSql/issues/18
            cn = new UnitTestsDbConnection(cn);
            string[] largeArray = new string[] { "table1", "table2", "table3", "table4", "table5", "table6", "table7", "table8", "table9", "table10", "table11" };
            var query = cn.QueryBuilder($"select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN {largeArray} OR COLUMN_NAME IN {largeArray}").Build();
            query.Execute();

            var dapperCommand = ((UnitTestsDbConnection)cn).PreviousCommands.Last();
            if (InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters)
            {
                Assert.AreEqual("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN @p0array OR COLUMN_NAME IN @p0array", query.Sql);
                Assert.AreEqual(1, query.SqlParameters.Count);

                // Dapper expands each occurrence of array @p0array into 11 elements
                Assert.AreEqual("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN (@p0array1,@p0array2,@p0array3,@p0array4,@p0array5,@p0array6,@p0array7,@p0array8,@p0array9,@p0array10,@p0array11) OR COLUMN_NAME IN (@p0array1,@p0array2,@p0array3,@p0array4,@p0array5,@p0array6,@p0array7,@p0array8,@p0array9,@p0array10,@p0array11)", dapperCommand.CommandText);
                Assert.AreEqual(11, dapperCommand.Parameters.Count);
            }
            else
            {
                Assert.AreEqual("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN @p0array OR COLUMN_NAME IN @p1array", query.Sql);
                Assert.AreEqual(2, query.SqlParameters.Count);

                // Dapper expands each array (@p0array and @p1array) into 11 elements
                Assert.AreEqual("select * from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN (@p0array1,@p0array2,@p0array3,@p0array4,@p0array5,@p0array6,@p0array7,@p0array8,@p0array9,@p0array10,@p0array11) OR COLUMN_NAME IN (@p1array1,@p1array2,@p1array3,@p1array4,@p1array5,@p1array6,@p1array7,@p1array8,@p1array9,@p1array10,@p1array11)", dapperCommand.CommandText);
                Assert.AreEqual(22, dapperCommand.Parameters.Count);
            }
        }

        [Test]
        public void DapperCollisionTest2()
        {
            if (InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters == true) return;

            //https://github.com/Drizin/InterpolatedSql/issues/18
            string[] test = new string[] { "test", "test2" };
            FormattableString sql = @$"
                select * from INFORMATION_SCHEMA.COLUMNS
                where {1} = 0 
                    or PlainText in {test} 
                    or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0 or {1} = 0
                    or PlainText in {test.Skip(2)}
                ";
            cn = new UnitTestsDbConnection(cn);
            string[] largeArray = new string[] { "table1", "table2", "table3", "table4", "table5", "table6", "table7", "table8", "table9", "table10", "table11" };
            var query = cn.QueryBuilder(sql).Build();
            try { query.Execute(); } catch { }
            var dapperCommand = ((UnitTestsDbConnection)cn).PreviousCommands.Last();
            
            string expected = @"
                select * from INFORMATION_SCHEMA.COLUMNS
                where @p0 = 0 
                    or PlainText in (@p1array1,@p1array2) 
                    or @p2 = 0 or @p3 = 0 or @p4 = 0 or @p5 = 0 or @p6 = 0 or @p7 = 0 or @p8 = 0 or @p9 = 0 or @p10 = 0
                    or PlainText in (SELECT @p11array WHERE 1 = 0)
                ";
            Assert.AreEqual(expected, dapperCommand.CommandText);
            Assert.AreEqual(13, dapperCommand.Parameters.Count); // 10 dummy scalars, 1 array expanded into 2 elements, 1 array with single null element
            // Previously Dapper was rendering "PlainText in (@parray11,@parray12)  ... or PlainText in (SELECT @parray11 WHERE 1 = 0)"
        }

        [Test]
        public void SimpleQueryStoredProcedure()
        {
            var q = cn.SqlBuilder($"[dbo].[uspGetEmployeeManagers]")
                .AddParameter("BusinessEntityID", 280);

            var r = q.Query<dynamic>(commandType: CommandType.StoredProcedure);
            int count = r.Count();
            Assert.That(count > 0);
        }

        [Test]
        public void QueryMultipleStoredProcedure()
        {
            //AdventureWorks does not contain a proc which returns multiple result sets, so create our own
            int a = cn.SqlBuilder($@"
CREATE OR ALTER PROCEDURE [dbo].[uspGetEmployeeManagers_Twice]
    @BusinessEntityID [int]
AS
BEGIN
    EXEC [dbo].[uspGetEmployeeManagers] @BusinessEntityID = @BusinessEntityID
    EXEC [dbo].[uspGetEmployeeManagers] @BusinessEntityID = @BusinessEntityID

END").Execute();
            var q = cn.SqlBuilder($"[dbo].[uspGetEmployeeManagers_Twice]")
                .AddParameter("BusinessEntityID", 280);

            using (var gridReader = q.QueryMultiple(commandType: CommandType.StoredProcedure))
            {
                var r = gridReader.Read<dynamic>();
                int count = r.Count();
                Assert.That(count > 0);
                r = gridReader.Read<dynamic>();
                Assert.AreEqual(count, r.Count());
            }
        }

    }
}
