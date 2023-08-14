using System;
using System.Data;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> but also wraps a (required) underlying IDbConnection, 
    /// has a "Filters" property which can track a list of filters which are later combined (by default with AND) and will replace the keyword /**where**/,
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps <see cref="IInterpolatedSql.SqlParameters"/> and <see cref="IInterpolatedSql.ExplicitParameters"/>
    /// into Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public class QueryBuilder<T> : InterpolatedSql.QueryBuilder<T>, IDapperSqlCommand<T>
        where T : QueryBuilder<T>
    {
        #region Members
        private ParametersDictionary? _cachedDapperParameters = null;

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => _cachedDapperParameters ??= ParametersDictionary.LoadFrom(this);
        #endregion

        #region ctors
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : base(connection)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(connection, query)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }
        #endregion

        #region Overrides
        /// <inheritdoc />
        protected override void PurgeParametersCache()
        {
            base.PurgeParametersCache();
            _cachedDapperParameters = null;
        }

        /// <summary>
        /// Associated DbConnection
        /// </summary>
        public new IDbConnection DbConnection
        {
            get => base.DbConnection!;
            set => base.DbConnection = value;
        }
        #endregion

    }

    /// <inheritdoc/>
    public class QueryBuilder : QueryBuilder<QueryBuilder>
    {
        #region ctors
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : base(connection)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(connection, query)
        {
        }
        #endregion
    }
}
