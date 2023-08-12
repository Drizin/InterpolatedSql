using InterpolatedSql.FluentQueryBuilder;
using System;
using System.Data;

namespace InterpolatedSql.Dapper.FluentQueryBuilder
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSql.FluentQueryBuilder.FluentQueryBuilder{T}"/> 
    /// (an injection-safe dynamic SQL builder with a Fluent API that helps to build the query step by step)
    /// but also wraps an underlying IDbConnection, and there are extensions to invoke Dapper methods
    /// </summary>
    public class FluentQueryBuilder
        : InterpolatedSql.FluentQueryBuilder.FluentQueryBuilder<FluentQueryBuilder>,
        IFluentQueryBuilder<FluentQueryBuilder>
        ,IDapperSqlCommand
    {
        #region Members
        private ParametersDictionary? _cachedDapperParameters = null;

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => _cachedDapperParameters ??= ParametersDictionary.LoadFrom(this);
        #endregion

        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder(IDbConnection connection) : base(connection)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc/>
        public FluentQueryBuilder(IDbConnection connection, FormattableString query) : base(connection, query)
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

}
