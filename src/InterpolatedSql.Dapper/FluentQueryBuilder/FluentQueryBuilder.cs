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
    public class FluentQueryBuilder<T>
        : InterpolatedSql.FluentQueryBuilder.FluentQueryBuilder<T>,
        IFluentQueryBuilder<T>
        ,IDapperSqlCommand
        where T : FluentQueryBuilder<T>, IFluentQueryBuilder<T>
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
        #endregion

        #region Overrides
        /// <inheritdoc />
        protected override void ClearParametersCache()
        {
            base.ClearParametersCache();
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
    public class FluentQueryBuilder : FluentQueryBuilder<FluentQueryBuilder>
    {
        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder(IDbConnection connection) : base(connection)
        {
        }
        #endregion

    }

}
