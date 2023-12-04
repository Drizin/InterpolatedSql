using System.Collections.Generic;
using System.Text;
using System;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc/>
    /// <remarks>This is just a simplification of InterpolatedSqlBuilder{U, R} where R is IInterpolatedSql (if you don't need a custom return type)</remarks>
    public abstract class SqlBuilder<U> : SqlBuilder<U, IInterpolatedSql> // this is just a simplification of InterpolatedSqlBuilder<U, R> where R is IInterpolatedSql (if you don't need a custom return type)
        where U : SqlBuilder<U, IInterpolatedSql>, ISqlBuilder<U, IInterpolatedSql>
    {
        #region ctors
        /// <inheritdoc />
        protected internal SqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments)
        {
        }

        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions? options = null) : base(options)
        {
        }


        /// <inheritdoc />
        public SqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : base(value, options)
        {
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options)
        {
        }
#endif
        #endregion
    }
}