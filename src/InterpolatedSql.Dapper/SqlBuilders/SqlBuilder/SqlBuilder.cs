using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// InterpolatedSqlBuilder is a dynamic SQL builder (but injection safe) where SqlParameters are defined using string interpolation.
    /// Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
    /// and will be parametrized when you need to run the command.
    /// So it wraps the underlying SQL statement and the associated parameters, 
    /// allowing to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    public class SqlBuilder : SqlBuilder<SqlBuilder>
    {
        #region ctor
        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
            : base(connection, options, format, arguments)
        {
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

    }


}