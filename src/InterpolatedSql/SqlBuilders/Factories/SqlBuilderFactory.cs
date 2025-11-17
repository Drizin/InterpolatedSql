using System;

namespace InterpolatedSql.SqlBuilders
{
    /// <summary>
    /// Factory to create instances of <see cref="SqlBuilder"/>
    /// </summary>
    public class SqlBuilderFactory : ISqlBuilderFactory
    {
        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create()
        {
            var builder = new SqlBuilder();
            return builder;
        }

        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(InterpolatedSqlBuilderOptions options)
        {
            var builder = new SqlBuilder(options);
            return builder;
        }



#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (SqlBuilder)value.InterpolatedSqlBuilder; // InterpolatedSqlHandler might take any IInterpolatedSqlBuilderBase, but can only create SqlBuilder
        }

        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (SqlBuilder)value.InterpolatedSqlBuilder; // InterpolatedSqlHandler might take any IInterpolatedSqlBuilderBase, but can only create SqlBuilder
        }

        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
        {
            SqlBuilder builder = new SqlBuilder(literalLength, formattedCount, options);
            return builder;
        }
#endif
        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            var builder = new SqlBuilder(value);
            return builder;
        }

        /// <summary>
        /// Creates a new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(InterpolatedSqlBuilderOptions options, FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            var builder = new SqlBuilder(value, options);
            return builder;
        }



        /// <summary>
        /// Default Factory
        /// </summary>
        public static SqlBuilderFactory Default = new SqlBuilderFactory();

    }
}
