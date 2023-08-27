﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.FluentQueryBuilder
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.FluentQueryBuilder.FluentQueryBuilder{U, RB, R}"/> 
    /// (an injection-safe dynamic SQL builder with a Fluent API that helps to build the query step by step)
    /// but also wraps an underlying IDbConnection, and there are extensions to invoke Dapper methods
    /// </summary>
    public class FluentQueryBuilder : global::InterpolatedSql.FluentQueryBuilder.FluentQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>,
        IFluentQueryBuilder,
        IBuildable<IDapperSqlCommand>,
        IDapperSqlCommand //TODO: deprecate this - users should call *Build()*.DapperExtensions<etc>
    {
        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, SqlBuilder> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, SqlBuilder> combinedBuilderFactory2,
            IDbConnection connection) : base(combinedBuilderFactory1, combinedBuilderFactory2, connection)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }
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
        public override IDapperSqlCommand Build()
        {
            var cmd = base.Build();
            if (cmd != null)
                cmd.DbConnection = this.DbConnection;
            return cmd;
        }
        #endregion
        #region IDapperSqlCommand - for Legacy compatibility... but not ideal because Build() will be called multiple times!
        ParametersDictionary IDapperSqlCommand.DapperParameters => ParametersDictionary.LoadFrom(this.Build());

        string IInterpolatedSql.Sql => this.Build().Sql;

        string IInterpolatedSql.Format => this.Build().Format;

        IReadOnlyList<InterpolatedSqlParameter> IInterpolatedSql.SqlParameters => this.Build().SqlParameters;

        IReadOnlyList<SqlParameterInfo> IInterpolatedSql.ExplicitParameters => this.Build().ExplicitParameters;
        #endregion
    }

}
