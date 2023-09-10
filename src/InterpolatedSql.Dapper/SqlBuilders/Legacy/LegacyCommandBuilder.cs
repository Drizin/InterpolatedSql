using InterpolatedSql.Dapper.SqlBuilders.InterpolatedSqlBuilder;
using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.SqlBuilders.Legacy
{
    /// <summary>
    /// Legacy compatibility. Exactly like parent class, but implements IDapperSqlCommand 
    /// </summary>
    public class LegacyCommandBuilder : SqlBuilder<LegacyCommandBuilder, IDapperSqlCommand>, IDapperSqlCommand, IInterpolatedSql, IInterpolatedSqlBuilderBase
    {

        #region ctors
        /// <inheritdoc />
        protected LegacyCommandBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(connection, options, format, arguments)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public LegacyCommandBuilder(IDbConnection connection) : base(connection)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public LegacyCommandBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions options) : base(connection, options)
        {
            DbConnection = connection;
        }


        /// <inheritdoc />
        public LegacyCommandBuilder(IDbConnection connection, FormattableString value) : base(connection, value)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public LegacyCommandBuilder(IDbConnection connection, FormattableString value, InterpolatedSqlBuilderOptions options) : base(connection, value, options)
        {
            DbConnection = connection;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public LegacyCommandBuilder(IDbConnection connection, int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(connection, literalLength, formattedCount, options)
        {
            DbConnection = connection;
        }
#endif
        #endregion


        #region IInterpolatedSql
        string IInterpolatedSql.Sql => base.Sql;

        IReadOnlyList<InterpolatedSqlParameter> IInterpolatedSql.SqlParameters => base.SqlParameters;

        IReadOnlyList<SqlParameterInfo> IInterpolatedSql.ExplicitParameters => base.ExplicitParameters;
        string IInterpolatedSql.Format => this.Build().Format;

        // Also exposed as public properties
        /// <inheritdoc />
        public new string Sql => ((IInterpolatedSql)this).Sql;

        /// <inheritdoc />
        public new IReadOnlyList<InterpolatedSqlParameter> SqlParameters => ((IInterpolatedSql)this).SqlParameters;

        /// <inheritdoc />
        public new IReadOnlyList<SqlParameterInfo> ExplicitParameters => ((IInterpolatedSql)this).ExplicitParameters;
        #endregion

        #region IDapperSqlCommand - for Legacy compatibility... but not ideal because calling Build() explicitly is more consistent API
        ParametersDictionary IDapperSqlCommand.DapperParameters => ParametersDictionary.LoadFrom(this.Build());

        // Also exposed as public properties
        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => ((IDapperSqlCommand)this).DapperParameters;

        #endregion

        /// <inheritdoc/>
        public override IDapperSqlCommand Build()
        {
            var build = AsSql();
            return new ImmutableDapperCommand(DbConnection, build.Sql, build.Format, build.SqlParameters, build.ExplicitParameters);
        }
    }

}