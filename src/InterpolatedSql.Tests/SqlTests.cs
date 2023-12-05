using InterpolatedSql.SqlBuilders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
        public void TestFormatTypes()
        {
            // Couldn't pass in SqlBuilder as parameter in [TestCase] and couldn't dynamically create argument formats
            // so this was best way to test all the different formats without duplicating asserts over and over.
            var testCases = new[]
            {
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"string":String});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "string", "string.2" }:String};",
                    Value = new [] { "string", "string.2" },
                    Length = (int?)StringParameterInfo.DefaultLength,
                    IsAnsi = false,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"string_50":String(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "string_50", "string_50.2" }:String(50)};",
                    Value = new [] { "string_50", "string_50.2" },
                    Length = (int?)50,
                    IsAnsi = false,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nvarchar":nvarchar});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nvarchar", "nvarchar.2" }:nvarchar};",
                    Value = new [] { "nvarchar", "nvarchar.2" },
                    Length = (int?)StringParameterInfo.DefaultLength,
                    IsAnsi = false,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nvarchar_50":nvarchar(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nvarchar_50", "nvarchar_50.2" }:nvarchar(50)};",
                    Value = new [] { "nvarchar_50", "nvarchar_50.2" },
                    Length = (int?)50,
                    IsAnsi = false,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"string_fixed":StringFixedLength});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "string_fixed", "string_fixed.2" }:StringFixedLength};",
                    Value = new [] { "string_fixed", "string_fixed.2" },
                    Length = (int?)null,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"string_fixed_50":StringFixedLength(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "string_fixed_50", "string_fixed_50.2" }:StringFixedLength(50)};",
                    Value = new [] { "string_fixed_50", "string_fixed_50.2" },
					Length = (int?)50,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nchar":nchar});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nchar", "nchar.2" }:nchar};",
                    Value = new [] { "nchar", "nchar.2" },
                    Length = (int?)null,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nchar_50":nchar(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nchar_50", "nchar_50.2" }:nchar(50)};",
                    Value = new [] { "nchar_50", "nchar_50.2" },
					Length = (int?)50,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"text":text});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "text", "text.2" }:text};",
                    Value = new [] { "text", "text.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"varchar_max":varchar(max)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "varchar_max", "varchar_max.2" }:varchar(max)};",
                    Value = new [] { "varchar_max", "varchar_max.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"varchar_minus1":varchar(-1)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "varchar_minus1", "varchar_minus1.2" }:varchar(-1)};",
                    Value = new [] { "varchar_minus1", "varchar_minus1.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"ansistring":AnsiString});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "ansistring", "ansistring.2" }:AnsiString};",
                    Value = new [] { "ansistring", "ansistring.2" },
                    Length = (int?)StringParameterInfo.DefaultLength,
                    IsAnsi = true,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"ansistring_50":AnsiString(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "ansistring_50", "ansistring_50.2" }:AnsiString(50)};",
                    Value = new [] { "ansistring_50", "ansistring_50.2" },
					Length = (int?)50,
                    IsAnsi = true,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"varchar":varchar});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "varchar", "varchar.2" }:varchar};",
                    Value = new [] { "varchar", "varchar.2" },
                    Length = (int?)StringParameterInfo.DefaultLength,
                    IsAnsi = true,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"varchar_50":varchar(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "varchar_50", "varchar_50.2" }:varchar(50)};",
                    Value = new [] { "varchar_50", "varchar_50.2" },
					Length = (int?)50,
                    IsAnsi = true,
                    IsFixedLength = false
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"ansistring_fixed":AnsiStringFixedLength});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "ansistring_fixed", "ansistring_fixed.2" }:AnsiStringFixedLength};",
                    Value = new [] { "ansistring_fixed", "ansistring_fixed.2" },
                    Length = (int?)null,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"ansistring_fixed_50":AnsiStringFixedLength(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "ansistring_fixed_50", "ansistring_fixed_50.2" }:AnsiStringFixedLength(50)};",
                    Value = new [] { "ansistring_fixed_50", "ansistring_fixed_50.2" },
					Length = (int?)50,
                    IsAnsi = true,
                    IsFixedLength = true
                },	
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"char":char});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "char", "char.2" }:char};",
                    Value = new [] { "char", "char.2" },
                    Length = (int?)null,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"char_50":char(50)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "char_50", "char_50.2" }:char(50)};",
                    Value = new [] { "char_50", "char_50.2" },
					Length = (int?)50,
                    IsAnsi = true,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"ntext":ntext});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "ntext", "ntext.2" }:ntext};",
                    Value = new [] { "ntext", "ntext.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nvarchar_max":nvarchar(max)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nvarchar_max", "nvarchar_max.2" }:nvarchar(max)};",
                    Value = new [] { "nvarchar_max", "nvarchar_max.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = false,
                    IsFixedLength = true
                },
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({"nvarchar_minus1":nvarchar(-1)});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { "nvarchar_minus1", "nvarchar_minus1.2" }:nvarchar(-1)};",
                    Value = new [] { "nvarchar_minus1", "nvarchar_minus1.2" },
                    Length = (int?)int.MaxValue,
                    IsAnsi = false,
                    IsFixedLength = true
                },

				// Known 'string format' for non-string type
                new
                {
                    StringSql = (FormattableString)$"INSERT INTO [Table] (col1) VALUES ({new XElement( "Properties", new XAttribute( "value", 1 ) ):ntext});",
                    ListSql = (FormattableString)$"SELECT col1 FROM [Table] WHERE col1 IN {new [] { new XElement( "Properties", new XAttribute( "value", 1 ) ), new XElement( "Properties", new XAttribute( "value", 2) ) }:ntext};",
                    Value = new [] { new XElement( "Properties", new XAttribute( "value", 1 ) ).ToString(), new XElement( "Properties", new XAttribute( "value", 2 ) ).ToString() },
                    Length = (int?)int.MaxValue,
                    IsAnsi = false,
                    IsFixedLength = true
                }
            };

			// Math.Max(StringParameterInfo.DefaultLength, "string".Length)
			foreach( var testCase in testCases )
			{
	            var sb = new SqlBuilder( testCase.StringSql ).Build();
				Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", sb.Sql, $"Argument format test case {testCase.Value.First()} failed.  Generated Sql mismatch." );

				Assert.AreEqual(1, sb.SqlParameters.Count, $"Argument format test case {testCase.Value.First()} failed.  Parameter count mismatch.");
				var parameterInfo = ( sb.SqlParameters[0].Argument as StringParameterInfo )!;
				Assert.NotNull(parameterInfo, $"Argument format test case {testCase.Value.First()} failed.  Argument is not StringParameterInfo.");
				Assert.AreEqual(testCase.Value.First(), parameterInfo.Value, $"Argument format test case {testCase.Value.First()} failed.  Parameter value mismatch.");
				Assert.AreEqual(testCase.IsAnsi, parameterInfo.IsAnsi, $"Argument format test case {testCase.Value.First()} failed.  IsAnsi mismatch.");
				Assert.AreEqual(testCase.IsFixedLength, parameterInfo.IsFixedLength, $"Argument format test case {testCase.Value.First()} failed.  IsFixedLength mismatch.");
				Assert.AreEqual(testCase.Length ?? testCase.Value.First().Length, parameterInfo.Length, $"Argument format test case {testCase.Value.First()} failed.  Argument length mismatch.");

	            sb = new SqlBuilder( testCase.ListSql ).Build();
				Assert.AreEqual("SELECT col1 FROM [Table] WHERE col1 IN @p0;", sb.Sql, $"Argument format test case {testCase.Value.First()} failed.  Generated Sql mismatch." );

				Assert.AreEqual(1, sb.SqlParameters.Count, $"Argument format test case {testCase.Value.First()} failed.  Parameter count mismatch.");
                var parameterInfos = (sb.SqlParameters[0].Argument as IEnumerable<StringParameterInfo>)?.ToArray()!;
                Assert.NotNull(parameterInfos, $"Argument format test case {testCase.Value.First()} failed.  Argument is not StringParameterInfo[].");

                for (int i = 0; i < testCase.Value.Length; i++)
				{
					Assert.AreEqual(testCase.Value[ i ], parameterInfos[ i ].Value, $"Argument format test case {testCase.Value[ i ]} failed.  Parameter value mismatch.");
					Assert.AreEqual(testCase.IsAnsi, parameterInfos[ i ].IsAnsi, $"Argument format test case {testCase.Value[ i ]} failed.  IsAnsi mismatch.");
					Assert.AreEqual(testCase.IsFixedLength, parameterInfos[ i ].IsFixedLength, $"Argument format test case {testCase.Value[ i ]} failed.  IsFixedLength mismatch.");
					Assert.AreEqual(testCase.Length ?? testCase.Value[ i ].Length, parameterInfos[ i ].Length, $"Argument format test case {testCase.Value[ i ]} failed.  Argument length mismatch.");
                }
			}
        }
    }
}