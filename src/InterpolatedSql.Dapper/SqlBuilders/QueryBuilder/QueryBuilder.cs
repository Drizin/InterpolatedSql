using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper
{
    /// <inheritdoc/>
    public class QueryBuilder : QueryBuilder<QueryBuilder, SqlBuilder, IDapperSqlCommand>
    {
        #region ctors
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection, query)
        {
        }
        #endregion
    }
}
