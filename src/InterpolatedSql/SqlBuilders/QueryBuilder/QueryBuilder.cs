﻿using System;
using System.Data;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc/>
    public class QueryBuilder : QueryBuilder<QueryBuilder, ISqlBuilder<SqlBuilder, IInterpolatedSql>, IInterpolatedSql>
    {
        /// <inheritdoc/>
        public QueryBuilder() : base(opts => new SqlBuilder(opts), (opts, format, arguments) => new SqlBuilder(opts, format, arguments))
        {
        }
        
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : this()
        {
            DbConnection = connection;
        }

        /// <inheritdoc/>
        public QueryBuilder(FormattableString query) : base(opts => new SqlBuilder(opts), (opts, format, arguments) => new SqlBuilder(opts, format, arguments), query)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : this(query)
        {
            DbConnection = connection;
        }

    }

}
