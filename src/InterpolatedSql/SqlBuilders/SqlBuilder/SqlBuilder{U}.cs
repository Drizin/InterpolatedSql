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
    public class SqlBuilder<U> : SqlBuilder<U, IInterpolatedSql>
        where U : SqlBuilder<U>, IInterpolatedSqlBuilder<U, IInterpolatedSql>
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


        /// <summary>
        /// For "bare" builder implementations (like <see cref="SqlBuilder"/> or <see cref="SqlBuilder{T}"/>) this just returns the same instance,
        /// but casted (through a wrapper) to <see cref="IInterpolatedSql"/>
        /// </summary>
        public override IInterpolatedSql Build()
        {
            return base.AsSql();
        }
    }
}
