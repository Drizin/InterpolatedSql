using System;
using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.Legacy
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions
    {
        #region LegacyCommandBuilder
        /// <summary>
        /// Creates a new LegacyCommandBuilder over current connection
        /// </summary>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public static LegacyCommandBuilder LegacyCommandBuilder(this IDbConnection cnn, FormattableString query)
        {
            return new LegacyCommandBuilder(cnn, query);
        }

        /// <summary>
        /// Creates a new empty LegacyCommandBuilder over current connection
        /// </summary>
        public static LegacyCommandBuilder LegacyCommandBuilder(this IDbConnection cnn)
        {
            return new LegacyCommandBuilder(cnn);
        }

        #endregion

        #region LegacyQueryBuilder
        /// <summary>
        /// Creates a new LegacyQueryBuilder over current connection
        /// </summary>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public static LegacyQueryBuilder LegacyQueryBuilder(this IDbConnection cnn, FormattableString query)
        {
            return new LegacyQueryBuilder(cnn, query);
        }

        /// <summary>
        /// Creates a new empty LegacyQueryBuilder over current connection
        /// </summary>
        public static LegacyQueryBuilder LegacyQueryBuilder(this IDbConnection cnn)
        {
            return new LegacyQueryBuilder(cnn);
        }

        #endregion
    }
}
