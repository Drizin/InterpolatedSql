using System;
using System.Collections.Generic;
using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.Legacy
{
    public class LegacyQueryBuilder : QueryBuilder<LegacyQueryBuilder, ISqlBuilder, IDapperSqlCommand>, IDapperSqlCommand, IInterpolatedSql, IInterpolatedSqlBuilderBase
    {

        #region ctors
        /// <inheritdoc/>
        public LegacyQueryBuilder(IDbConnection connection) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection)
        {
        }

        /// <inheritdoc/>
        public LegacyQueryBuilder(IDbConnection connection, FormattableString query) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection, query)
        {
        }
        #endregion


        #region IInterpolatedSql
        string IInterpolatedSql.Sql => this.Build().Sql;

        string IInterpolatedSql.Format => this.Build().Format;

        IReadOnlyList<InterpolatedSqlParameter> IInterpolatedSql.SqlParameters => this.Build().SqlParameters;

        IReadOnlyList<SqlParameterInfo> IInterpolatedSql.ExplicitParameters => this.Build().ExplicitParameters;


        // Also exposed as public properties
        /// <inheritdoc />
        public new string Sql => ((IInterpolatedSql)this).Sql;

        /// <inheritdoc />
        public new IReadOnlyList<InterpolatedSqlParameter> SqlParameters => ((IInterpolatedSql)this).SqlParameters;

        /// <inheritdoc />
        public new IReadOnlyList<SqlParameterInfo> ExplicitParameters => ((IInterpolatedSql)this).ExplicitParameters;
        #endregion


        #region Overrides
        /// <summary>
        /// Associated DbConnection
        /// </summary>
        public new IDbConnection DbConnection
        {
            get => base.DbConnection!;
            set => base.DbConnection = value;
        }

        #endregion

        #region IDapperSqlCommand - for Legacy compatibility... but not ideal because Build() will be called multiple times!
        ParametersDictionary IDapperSqlCommand.DapperParameters => ParametersDictionary.LoadFrom(this.Build());

        // Also exposed as public properties
        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => ParametersDictionary.LoadFrom(this);
        #endregion

    }

}