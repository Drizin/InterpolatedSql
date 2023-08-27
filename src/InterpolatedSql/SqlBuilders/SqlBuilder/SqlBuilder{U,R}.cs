using System;
using System.Collections.Generic;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// SqlBuilder is a simple specialization of InterpolatedSqlBuilder (a dynamic SQL builder where SqlParameters are defined using string interpolation and yet it's but injection safe),
    /// the major addition is that it exposes some underlying protected properties (Sql/SqlParameters/ExplicitParameters) making it an implementation of IInterpolatedSql.
    /// (In other words you don't have to call Build() - you just pass the builder and it should be accepted by any method that expects an IInterpolatedSql.
    /// </summary>
    public abstract class SqlBuilder<U,R> : InterpolatedSqlBuilder<U, R>, IInterpolatedSql
        where U : IInterpolatedSqlBuilder<U, R>
        where R : class, IInterpolatedSql
    {
        #region ctor
        /// <inheritdoc />
        protected SqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments) { }

        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions? options = null) : base(options) { }


        /// <inheritdoc />
        public SqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : base(value, options) { }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options)
        {
        }
#endif


        #endregion

        #region IInterpolatedSql
        string IInterpolatedSql.Sql => base.Sql;

        IReadOnlyList<InterpolatedSqlParameter> IInterpolatedSql.SqlParameters => base.SqlParameters;

        IReadOnlyList<SqlParameterInfo> IInterpolatedSql.ExplicitParameters => base.ExplicitParameters;


        // Also exposed as public properties
        /// <inheritdoc />
        public new string Sql => ((IInterpolatedSql)this).Sql;

        /// <inheritdoc />
        public new IReadOnlyList<InterpolatedSqlParameter> SqlParameters => ((IInterpolatedSql)this).SqlParameters;

        /// <inheritdoc />
        public new IReadOnlyList<SqlParameterInfo> ExplicitParameters => ((IInterpolatedSql)this).ExplicitParameters;
        #endregion
    }
}
