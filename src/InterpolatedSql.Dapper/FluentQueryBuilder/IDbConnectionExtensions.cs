using System.Data;
using InterpolatedSql.FluentQueryBuilder;

namespace InterpolatedSql.Dapper.FluentQueryBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions //TODO: all factories here could be delegated to a Factory class, so that we can replace the factory
    {
        #region Fluent Query Builder
        /// <summary>
        /// Creates a new empty FluentQueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static IEmptyQueryBuilder<FluentQueryBuilder> FluentQueryBuilder(this IDbConnection cnn)
        {
            return new FluentQueryBuilder(cnn);
        }
        #endregion
    }
}
