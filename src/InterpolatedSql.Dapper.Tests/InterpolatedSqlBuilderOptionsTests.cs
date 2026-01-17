using NUnit.Framework;
using InterpolatedSql.SqlBuilders;
using Microsoft.Data.SqlClient;

namespace InterpolatedSql.Dapper.Tests;

public class InterpolatedSqlBuilderOptionsTests
{
    [Test]
    public void SqlBuilderWithOptions()
    {
        var cn = new SqlConnection();
        var qb = cn.SqlBuilder(new InterpolatedSqlBuilderOptions { DatabaseParameterSymbol = ":" }, $"{"A"} {1}");
        var sql = qb.Build().Sql;
        Assert.AreEqual(":p0 :p1", sql);
    }

    [Test]
    public void QueryBuilderWithOptions()
    {
        var cn = new SqlConnection();
        var qb = cn.SqlBuilder<InterpolatedSql.Dapper.SqlBuilders.QueryBuilder>(new InterpolatedSqlBuilderOptions { DatabaseParameterSymbol = ":" }, $"{"A"} {1}");
        var sql = qb.Build().Sql;
        Assert.AreEqual(":p0 :p1", sql);
    }
}