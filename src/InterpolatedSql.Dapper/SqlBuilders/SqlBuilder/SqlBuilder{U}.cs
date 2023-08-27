using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.SqlBuilder{U, R}"/> 
    /// (an injection-safe dynamic SQL builder with a Fluent API that helps to build the query step by step, and implements IInterpolatedSql)
    /// but also wraps an underlying IDbConnection, and there are extensions to invoke Dapper methods.
    /// Different than other Dapper specializations like <see cref="QueryBuilder"/> or <see cref="FluentQueryBuilder.FluentQueryBuilder"/>, this one also implements <see cref="IDapperSqlCommand"/>,
    /// which means that it can automatically run Dapper extensions found in <see cref="IDapperSqlCommandExtensions"/>
    /// </summary>
    public abstract class SqlBuilder<U> : global::InterpolatedSql.SqlBuilder<U, IDapperSqlCommand>, IInterpolatedSqlBuilder<U, IDapperSqlCommand>, IDapperSqlCommand
        where U : SqlBuilder<U>, IInterpolatedSqlBuilder<U, IDapperSqlCommand>
    {
        #region ctor
        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
            : base(options, format, arguments)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null) : this(connection: connection, options: options, format: null, arguments: null)
        {
        }


        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(connection: connection, options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be useful to provide Options (and Parser) immediately together
            if (value != null)
                Options.Parser.ParseAppend(this, value);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
            : base(literalLength, formattedCount, options)
        {

        }
#endif

        #endregion

        #region Overrides
        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => ParametersDictionary.LoadFrom(this);

        /// <inheritdoc />
        public override IDapperSqlCommand Build()
        {
            return this;
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