using System;
using System.Data;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions //TODO: all factories here could be delegated to a Factory class, so that we can replace the factory
    {
        #region SqlBuilder
#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, ref InterpolatedSqlHandler command)
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return new SqlBuilder(cnn, command.InterpolatedSqlBuilder);
        }
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler command)
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return new SqlBuilder(cnn, command.InterpolatedSqlBuilder);
        }

#else
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new SqlBuilder(cnn, command);
        }
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, FormattableString command)
        {
            return new SqlBuilder(cnn, command, options);
        }
#endif

        /// <summary>
        /// Creates a new empty SqlBuilder over current connection
        /// </summary>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions? options = null)
        {
            return new SqlBuilder(cnn, options);
        }
        #endregion

        #region QueryBuilder
        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public static QueryBuilder QueryBuilder(this IDbConnection cnn, FormattableString query)
        {
            return new QueryBuilder(cnn, query);
        }

        /// <summary>
        /// Creates a new empty QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static QueryBuilder QueryBuilder(this IDbConnection cnn)
        {
            return new QueryBuilder(cnn);
        }
        #endregion
    }
}
