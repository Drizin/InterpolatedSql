using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> but it wraps a (required) underlying IDbConnection, 
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps <see cref="IInterpolatedSql.SqlParameters"/> and <see cref="IInterpolatedSql.ExplicitParameters"/>
    /// into Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public interface ISqlBuilder : IInterpolatedSql
    {
    }

    /// <inheritdoc />
    /// <typeparam name="T">The concrete type that implements this interface. Useful for Fluent APIs</typeparam>
    public interface ISqlBuilder<T> : ISqlBuilder, IInterpolatedSql<T>, ISqlCommand<T>, IDapperSqlCommand<T>
        where T : ISqlBuilder<T>
    {

    }

    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> but it requires an underlying IDbConnection, 
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps to Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public class SqlBuilder : InterpolatedSqlBuilder<SqlBuilder>, ISqlCommand<SqlBuilder>, ISqlBuilder<SqlBuilder>
    {
        #region Members
        private ParametersDictionary? _cachedDapperParameters = null;

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => _cachedDapperParameters ??= ParametersDictionary.LoadFrom(this);
        #endregion

        #region ctors
        /// <inheritdoc cref="SqlBuilder" />
        protected internal SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments) 
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc cref="SqlBuilder" />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null) : base(options)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn">Underlying connection</param>
        /// <param name="command">SQL command</param>
        public SqlBuilder(IDbConnection cnn, FormattableString command, InterpolatedSqlBuilderOptions? options = null) : this(cnn, options)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            AppendFormattableString(command);
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn">Underlying connection</param>
        /// <param name="command">SQL command</param>
        public SqlBuilder(IDbConnection cnn, InterpolatedSqlBuilder command, InterpolatedSqlBuilderOptions? options = null) : this(cnn, options)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            Append(command);
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
