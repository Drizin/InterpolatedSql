using NUnit.Framework;

namespace InterpolatedSql.Tests
{
    public class SqlTests
    {

        [Test]
        public void Test1() //TODO: move tests from DapperQueryBuilder.Tests to this project.
        {
            int val = 1;
            var s1 = new SqlBuilder($"INSERT INTO [Table] (col1) VALUES ({val});");
            var s2 = new SqlBuilder($"INSERT INTO [Table] (col1) VALUES ('{val}');");
            var s3 = new SqlBuilder($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});");
            SqlBuilder.DefaultOptions.ReuseIdenticalParameters = true;
            var s4 = new SqlBuilder($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});");
            var s5 = InterpolatedSqlFactory.Default.Create($"INSERT INTO [Table] (col1, col2) VALUES ({val}, {val});");

            Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", s1.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", s2.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p1);", s3.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p0);", s4.Sql);
            Assert.AreEqual("INSERT INTO [Table] (col1, col2) VALUES (@p0, @p0);", s5.Build().Sql);

            Assert.AreEqual(1, s1.SqlParameters.Count);
            Assert.AreEqual(val, s1.SqlParameters[0].Argument);

            Assert.AreEqual(1, s2.SqlParameters.Count);
            Assert.AreEqual(val, s2.SqlParameters[0].Argument);
        }

    }
}