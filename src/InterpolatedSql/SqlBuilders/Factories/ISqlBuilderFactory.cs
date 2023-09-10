using System;

namespace InterpolatedSql.SqlBuilders
{
    public interface ISqlBuilderFactory
    {
        SqlBuilder Create();
        SqlBuilder Create(InterpolatedSqlBuilderOptions options);
#if NET6_0_OR_GREATER
        SqlBuilder Create(ref InterpolatedSqlHandler value);
        SqlBuilder Create(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler value);
        SqlBuilder Create(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null);
#else
        SqlBuilder Create(FormattableString value);
        SqlBuilder Create(InterpolatedSqlBuilderOptions options, FormattableString value);
#endif
    }
}