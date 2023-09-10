using System.Collections.Generic;

namespace InterpolatedSql
{
    /// <summary>
    /// Immutable implementation of <see cref="IInterpolatedSql"/>.
    /// If you want a mutable class (to dynamically append statements) you probably need <see cref="ISqlBuilder"/> or some builder
    /// </summary>
    public class ImmutableInterpolatedSql : IInterpolatedSql
    {
        /// <inheritdoc cref="InterpolatedSql"/>
        public ImmutableInterpolatedSql(string sql, string format, IReadOnlyList<InterpolatedSqlParameter> sqlParameters, IReadOnlyList<SqlParameterInfo> explicitParameters)
        {
            Sql = sql;
            Format = format;
            SqlParameters = sqlParameters;
            ExplicitParameters = explicitParameters;
        }

        /// <inheritdoc cref="IInterpolatedSql.Format"/>
        public string Format { get; private set; }

        /// <inheritdoc cref="IInterpolatedSql.SqlParameters"/>
        public IReadOnlyList<InterpolatedSqlParameter> SqlParameters { get; private set; }

        /// <inheritdoc cref="IInterpolatedSql.ExplicitParameters"/>
        public IReadOnlyList<SqlParameterInfo> ExplicitParameters { get; private set; }

        /// <inheritdoc cref="IInterpolatedSql.Sql"/>
        public string Sql { get; private set; }
    }
}