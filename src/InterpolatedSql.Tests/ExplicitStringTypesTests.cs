using InterpolatedSql.SqlBuilders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace InterpolatedSql.Tests
{
    public class ExplicitStringTypesTests
    {
        public class TestFormatInput
        {
            internal FormattableString StringSql;
            internal FormattableString ListSql;
            internal string[] Value;
            internal int? Length;
            internal bool IsAnsi;
            internal bool IsFixedLength;
        }
        private static IEnumerable<TestFormatInput> GetFormatTestCases()
        {
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"string":String});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "string", "string.2" }:String};",
                Value = new[] { "string", "string.2" },
                Length = StringParameterInfo.DefaultLength,
                IsAnsi = false,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"string_50":String(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "string_50", "string_50.2" }:String(50)};",
                Value = new[] { "string_50", "string_50.2" },
                Length = 50,
                IsAnsi = false,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nvarchar":nvarchar});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nvarchar", "nvarchar.2" }:nvarchar};",
                Value = new[] { "nvarchar", "nvarchar.2" },
                Length = StringParameterInfo.DefaultLength,
                IsAnsi = false,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nvarchar_50":nvarchar(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nvarchar_50", "nvarchar_50.2" }:nvarchar(50)};",
                Value = new[] { "nvarchar_50", "nvarchar_50.2" },
                Length = 50,
                IsAnsi = false,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"string_fixed":StringFixedLength});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "string_fixed", "string_fixed.2" }:StringFixedLength};",
                Value = new[] { "string_fixed", "string_fixed.2" },
                Length = null,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"string_fixed_50":StringFixedLength(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "string_fixed_50", "string_fixed_50.2" }:StringFixedLength(50)};",
                Value = new[] { "string_fixed_50", "string_fixed_50.2" },
                Length = 50,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nchar":nchar});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nchar", "nchar.2" }:nchar};",
                Value = new[] { "nchar", "nchar.2" },
                Length = null,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nchar_50":nchar(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nchar_50", "nchar_50.2" }:nchar(50)};",
                Value = new[] { "nchar_50", "nchar_50.2" },
                Length = 50,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"text":text});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "text", "text.2" }:text};",
                Value = new[] { "text", "text.2" },
                Length = int.MaxValue,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"varchar_max":varchar(max)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "varchar_max", "varchar_max.2" }:varchar(max)};",
                Value = new[] { "varchar_max", "varchar_max.2" },
                Length = int.MaxValue,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"varchar_minus1":varchar(-1)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "varchar_minus1", "varchar_minus1.2" }:varchar(-1)};",
                Value = new[] { "varchar_minus1", "varchar_minus1.2" },
                Length = int.MaxValue,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"ansistring":AnsiString});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "ansistring", "ansistring.2" }:AnsiString};",
                Value = new[] { "ansistring", "ansistring.2" },
                Length = StringParameterInfo.DefaultLength,
                IsAnsi = true,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"ansistring_50":AnsiString(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "ansistring_50", "ansistring_50.2" }:AnsiString(50)};",
                Value = new[] { "ansistring_50", "ansistring_50.2" },
                Length = 50,
                IsAnsi = true,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"varchar":varchar});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "varchar", "varchar.2" }:varchar};",
                Value = new[] { "varchar", "varchar.2" },
                Length = StringParameterInfo.DefaultLength,
                IsAnsi = true,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"varchar_50":varchar(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "varchar_50", "varchar_50.2" }:varchar(50)};",
                Value = new[] { "varchar_50", "varchar_50.2" },
                Length = 50,
                IsAnsi = true,
                IsFixedLength = false
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"ansistring_fixed":AnsiStringFixedLength});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "ansistring_fixed", "ansistring_fixed.2" }:AnsiStringFixedLength};",
                Value = new[] { "ansistring_fixed", "ansistring_fixed.2" },
                Length = null,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"ansistring_fixed_50":AnsiStringFixedLength(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "ansistring_fixed_50", "ansistring_fixed_50.2" }:AnsiStringFixedLength(50)};",
                Value = new[] { "ansistring_fixed_50", "ansistring_fixed_50.2" },
                Length = 50,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"char":char});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "char", "char.2" }:char};",
                Value = new[] { "char", "char.2" },
                Length = null,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"char_50":char(50)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "char_50", "char_50.2" }:char(50)};",
                Value = new[] { "char_50", "char_50.2" },
                Length = 50,
                IsAnsi = true,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"ntext":ntext});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "ntext", "ntext.2" }:ntext};",
                Value = new[] { "ntext", "ntext.2" },
                Length = int.MaxValue,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nvarchar_max":nvarchar(max)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nvarchar_max", "nvarchar_max.2" }:nvarchar(max)};",
                Value = new[] { "nvarchar_max", "nvarchar_max.2" },
                Length = int.MaxValue,
                IsAnsi = false,
                IsFixedLength = true
            };
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({"nvarchar_minus1":nvarchar(-1)});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { "nvarchar_minus1", "nvarchar_minus1.2" }:nvarchar(-1)};",
                Value = new[] { "nvarchar_minus1", "nvarchar_minus1.2" },
                Length = int.MaxValue,
                IsAnsi = false,
                IsFixedLength = true
            };

            // If a non-string type is passed to a explicit string type then it's serialized
            yield return new TestFormatInput()
            {
                StringSql = $"INSERT INTO [Table] (col1) VALUES ({new XElement("Properties", new XAttribute("value", 1)):ntext});",
                ListSql = $"SELECT col1 FROM [Table] WHERE col1 IN {new[] { new XElement("Properties", new XAttribute("value", 1)), new XElement("Properties", new XAttribute("value", 2)) }:ntext};",
                Value = new[] { new XElement("Properties", new XAttribute("value", 1)).ToString(), new XElement("Properties", new XAttribute("value", 2)).ToString() },
                Length = int.MaxValue,
                IsAnsi = false,
                IsFixedLength = true
            };

        }

        [TestCaseSource(nameof(GetFormatTestCases))]
        public void TestFormatTypes(TestFormatInput testCase)
        {
            var sb = new SqlBuilder(testCase.StringSql).Build();
            Assert.AreEqual("INSERT INTO [Table] (col1) VALUES (@p0);", sb.Sql, $"Argument format test case {testCase.Value.First()} failed.  Generated Sql mismatch.");

            Assert.AreEqual(1, sb.SqlParameters.Count, $"Argument format test case {testCase.Value.First()} failed.  Parameter count mismatch.");
            var parameterInfo = (sb.SqlParameters[0].Argument as StringParameterInfo)!;
            Assert.NotNull(parameterInfo, $"Argument format test case {testCase.Value.First()} failed.  Argument is not StringParameterInfo.");
            Assert.AreEqual(testCase.Value.First(), parameterInfo.Value, $"Argument format test case {testCase.Value.First()} failed.  Parameter value mismatch.");
            Assert.AreEqual(testCase.IsAnsi, parameterInfo.IsAnsi, $"Argument format test case {testCase.Value.First()} failed.  IsAnsi mismatch.");
            Assert.AreEqual(testCase.IsFixedLength, parameterInfo.IsFixedLength, $"Argument format test case {testCase.Value.First()} failed.  IsFixedLength mismatch.");
            Assert.AreEqual(testCase.Length ?? testCase.Value.First().Length, parameterInfo.Length, $"Argument format test case {testCase.Value.First()} failed.  Argument length mismatch.");

            sb = new SqlBuilder(testCase.ListSql).Build();
            Assert.AreEqual("SELECT col1 FROM [Table] WHERE col1 IN @p0;", sb.Sql, $"Argument format test case {testCase.Value.First()} failed.  Generated Sql mismatch.");

            Assert.AreEqual(1, sb.SqlParameters.Count, $"Argument format test case {testCase.Value.First()} failed.  Parameter count mismatch.");
            var parameterInfos = (sb.SqlParameters[0].Argument as IEnumerable<StringParameterInfo>)?.ToArray()!;
            Assert.NotNull(parameterInfos, $"Argument format test case {testCase.Value.First()} failed.  Argument is not StringParameterInfo[].");

            for (int i = 0; i < testCase.Value.Length; i++)
            {
                Assert.AreEqual(testCase.Value[i], parameterInfos[i].Value, $"Argument format test case {testCase.Value[i]} failed.  Parameter value mismatch.");
                Assert.AreEqual(testCase.IsAnsi, parameterInfos[i].IsAnsi, $"Argument format test case {testCase.Value[i]} failed.  IsAnsi mismatch.");
                Assert.AreEqual(testCase.IsFixedLength, parameterInfos[i].IsFixedLength, $"Argument format test case {testCase.Value[i]} failed.  IsFixedLength mismatch.");
                Assert.AreEqual(testCase.Length ?? testCase.Value[i].Length, parameterInfos[i].Length, $"Argument format test case {testCase.Value[i]} failed.  Argument length mismatch.");
            }
        }

    }
}