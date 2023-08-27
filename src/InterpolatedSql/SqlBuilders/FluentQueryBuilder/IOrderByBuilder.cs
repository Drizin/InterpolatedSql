using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more orderby clauses, which can still add more clauses to orderby
    /// </summary>
    public interface IOrderByBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds one column to orderby clauses.
        /// </summary>
        IOrderByBuilder<U, RB, R> OrderBy(FormattableString column);

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        ICompleteBuilder<U, RB, R> Limit(int offset, int rowCount);

        /// <summary>
        /// Creates final query
        /// </summary>
        /// <returns></returns>
        R Build();
    }
}
