using NUnit.Framework;
using System.Data;
using System;
using InterpolatedSql.Dapper.SqlBuilders;

namespace InterpolatedSql.Dapper.Tests
{
    /// <see cref="ParentClass"/>
    public class MyQueryBuilder : InterpolatedSql.Dapper.SqlBuilders.QueryBuilder<MyQueryBuilder, ISqlBuilder, IDapperSqlCommand>
    {
        public MyQueryBuilder(IDbConnection connection, FormattableString query) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection, query)
        {
        }
        public MyQueryBuilder AppendCustomObject(FormattableString value, string key, string separator, string prefixBeforeFirstItem = "")
        {
            if (!ObjectBag!.ContainsKey(key))
                ObjectBag[key] = new InterpolatedSql.SqlBuilders.SqlBuilder();
            var builder = (InterpolatedSql.SqlBuilders.SqlBuilder)base.ObjectBag[key];
            builder.AppendRaw(builder.IsEmpty ? prefixBeforeFirstItem : separator);
            builder.AppendFormattableString(value);
            return this;
        }
        public MyQueryBuilder From(FormattableString value, string key) => AppendCustomObject(value, "from:" + key, "\nINNER JOIN ");
        public MyQueryBuilder Select(FormattableString value, string key) => AppendCustomObject(value, "select:" + key, ", ");
        public MyQueryBuilder Where(FormattableString value, string key) => AppendCustomObject(value, "filters:" + key, " AND ");
        public MyQueryBuilder Add(string key, FormattableString value) => AppendCustomObject(value, key, "");
        public void ReplaceCustomObjects()
        {
            foreach(var key in base.ObjectBag!.Keys) 
                this.Replace("/**" + key + "**/", ((InterpolatedSql.SqlBuilders.SqlBuilder)base.ObjectBag![key]).Build());
        }
    }

    public class ExtensibilityTests
    {

        [Test]
        public void Test1()
        {
            IDbConnection cn = null!;
            string someValue = "hope it";
            string otherValue = "works";

            var q = new MyQueryBuilder(cn, $$"""
                select
                    MyInnerTable.*
                    /**selects**/
                from
                    (
                        select 
                            Id
                            /**select:somekeysuffix**/
                        from 
                            SomeTable
                            /**from:somekeysuffix**/
                        where
                            1=1
                            /**filters:somekeysuffix**/
                    ) MyInnerTable
                where
                    1=1
                    /**filters**/
                group by
                    /**groupby**/
                order by
                    /**orderby**/

                /**myCustomUnion**/
                """)
                    .From($"join OtherTable t on SomeTable.OtherTableId=t.Id", "somekeysuffix")
                    .Select($"morecolumn")
                    .Select($"SomeColumn", "somekeysuffix")
                    .Where($"SomeColumn={someValue}", "somekeysuffix")
                    .Where($"OtherColumn={otherValue}")
                    .GroupBy($"SomeColumn")
                    .OrderBy($"SomeColumn")
                    .Replace("/**myCustomUnion**/", $"SomeArbitrarySql");
            ;
            q.ReplaceCustomObjects();
            var cmd = q.Build();

            Assert.AreEqual("""
                select
                    MyInnerTable.*
                    , morecolumn
                from
                    (
                        select 
                            Id
                            SomeColumn
                        from 
                            SomeTable
                            join OtherTable t on SomeTable.OtherTableId=t.Id
                        where
                            1=1
                            SomeColumn=@p0
                    ) MyInnerTable
                where
                    1=1
                    AND OtherColumn=@p1
                group by
                    GROUP BY SomeColumn
                order by
                    ORDER BY SomeColumn

                SomeArbitrarySql
                """, cmd.Sql);

            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p0"));
            Assert.That(cmd.DapperParameters.ParameterNames.Contains("p1"));
            Assert.AreEqual(someValue, cmd.DapperParameters["p0"].Value);
            Assert.AreEqual(otherValue, cmd.DapperParameters["p1"].Value);
        }
    }
    public class ExtensibilityTests2 
    { 
        public class MyCustomSqlParameterMapper : SqlParameterMapper
        {
            public override string CalculateAutoParameterName(InterpolatedSqlParameter parameter, int pos, InterpolatedSql.SqlBuilders.InterpolatedSqlBuilderOptions options)
            {
            	// InterpolatedSql.Dapper.SqlParameterMapper returns this
                //return 
                //    //options.AutoGeneratedParameterPrefix +
                //    parameter.Format! +
                //    (base.IsEnumerable(parameter.Argument) ? options.ParameterArrayNameSuffix : "") +
                //    pos.ToString();
                
                // But if we want to give explicit names:
                return parameter.Format!.Split( ':' )[ 0 ] + (base.IsEnumerable(parameter.Argument) ? options.ParameterArrayNameSuffix : "");
            }
        }
        [SetUp]
        public void Setup()
        {
            // This would work with pure InterpolatedSql
            //InterpolatedSql.SqlBuilders.InterpolatedSqlBuilderOptions.DefaultOptions.CalculateAutoParameterName = (p, pos) => p.Format!; 

            // But InterpolatedSql.Dapper builders will override CalculateAutoParameterName using InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser, so we can subclass that
            InterpolatedSqlDapperOptions.DefaultOptions = new InterpolatedSqlDapperOptions()
            {
                InterpolatedSqlParameterParser = new MyCustomSqlParameterMapper()
            };
        }

        [Test]
        [NonParallelizable]
        public void Test2()
        {
            DateTime asOfDate = DateTime.Now;
            IDbConnection cn = null!;

            var query = cn.QueryBuilder($"Select * from Users Where ModifiedDate >= {asOfDate:asOfDate}").Build();
            Assert.AreEqual("Select * from Users Where ModifiedDate >= @asOfDate", query.Sql);

            var name = "John";
            query = cn.QueryBuilder($"Select * from Users Where Name = {name:userName:varchar(200)}").Build();
            Assert.AreEqual("Select * from Users Where Name = @userName", query.Sql);
            var parameterInfo = (query.SqlParameters[0].Argument as StringParameterInfo)!;

            Assert.NotNull(parameterInfo, "Argument is not StringParameterInfo.");
            Assert.AreEqual(name, parameterInfo.Value, "Parameter value mismatch.");
            Assert.AreEqual(true, parameterInfo.IsAnsi, "IsAnsi mismatch.");
            Assert.AreEqual(false, parameterInfo.IsFixedLength, "IsFixedLength mismatch.");
            Assert.AreEqual(200, parameterInfo.Length, "Argument length mismatch.");

            InterpolatedSqlDapperOptions.DefaultOptions = new InterpolatedSqlDapperOptions();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            InterpolatedSqlDapperOptions.DefaultOptions = new InterpolatedSqlDapperOptions();
        }

    }
}
