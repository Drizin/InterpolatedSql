using InterpolatedSql.SqlBuilders;
using NUnit.Framework;

namespace InterpolatedSql.Tests
{
    public class SqlTests
    {

        [Test]
        public void Test1() //TODO: move some tests from InterpolatedSql.Dapper.Tests to this project.
        {
            int val = 1;
            var s1 = new SqlBuilder($"INSERT INTO [Table] (col1) VALUES ({val});").Build();
            var s2 = new SqlBuilder($"INSERT INTO [Table] (col1) VALUES ('{val}');").Build();
            var s3 = new SqlBuilder($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});").Build();
            InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters = true;
            var s4 = new SqlBuilder($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});").Build();
            var s5 = SqlBuilderFactory.Default.Create($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});").Build();

            Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", s1.Sql);
            Assert.AreEqual(1, s1.SqlParameters.Count);
            Assert.AreEqual(val, s1.SqlParameters[0].Argument);

            Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", s2.Sql);
            Assert.AreEqual(1, s2.SqlParameters.Count);
            Assert.AreEqual(val, s2.SqlParameters[0].Argument);

            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p1);", s3.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p0);", s4.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p0);", s5.Sql);
        }

        [Test]
        public void Test2()
        {
            var qb = new QueryBuilder(@$"
	            SELECT COUNT(*) TotalCount 
	            FROM [File] f 
		            INNER JOIN Folder fo ON f.Folder = fo.UID 
	            WHERE f.Deleted != 1 /**filters**/
            ");

            qb.Where($"Price>{1}");
            qb.Where($"Price<{10}");

            var b = qb.Build();
            System.Diagnostics.Debug.Write(b.Sql);
            Assert.AreEqual(@$"
	            SELECT COUNT(*) TotalCount 
	            FROM [File] f 
		            INNER JOIN Folder fo ON f.Folder = fo.UID 
	            WHERE f.Deleted != 1 AND Price>@p0 AND Price<@p1
            ", b.Sql);
            Assert.AreEqual(2, b.SqlParameters.Count);
            Assert.AreEqual(1, b.SqlParameters[0].Argument);
            Assert.AreEqual(10, b.SqlParameters[1].Argument);
        }
 
         [Test]
        public void TestMultipleFilterExtensions()
        {
            InterpolatedSqlBuilderOptions.DefaultOptions.ReuseIdenticalParameters = false;
            var storageFolder = "_CALCENGINES";
			var sqlTestNamePattern = "'%~_Test.%' ESCAPE '~'";
            var regularNames = new[] { "Conduent_1_CE" };

            var qb = new QueryBuilder(@$"
				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = {storageFolder:varchar(200)} AND f.Name NOT LIKE {sqlTestNamePattern:raw} AND f.Deleted != 1 /**filters**/

				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = {storageFolder:varchar(200)} AND f.Name NOT LIKE {sqlTestNamePattern:raw} AND f.Deleted != 1 /**filters**/
			");

			var filters = new Filters(Filters.FiltersType.AND)
            {
                new Filter($"f.Name IN {regularNames:varchar(200)}")
            };

            qb.Where(filters);

            var sql = qb.Build();

            Assert.AreEqual(@"
				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = @p0 AND f.Name NOT LIKE '%~_Test.%' ESCAPE '~' AND f.Deleted != 1 AND f.Name IN @p2

				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = @p1 AND f.Name NOT LIKE '%~_Test.%' ESCAPE '~' AND f.Deleted != 1 AND f.Name IN @p3
			", sql.Sql);
        }

    }
}