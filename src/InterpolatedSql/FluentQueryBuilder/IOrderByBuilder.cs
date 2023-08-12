using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more orderby clauses, which can still add more clauses to orderby
    /// </summary>
    public interface IOrderByBuilder<T>
        : ISqlCommand<T>
        where T : IFluentQueryBuilder<T>
    {
        /// <summary>
        /// Adds one column to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString column);

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        ICompleteBuilder<T> Limit(int offset, int rowCount);
    }
}
