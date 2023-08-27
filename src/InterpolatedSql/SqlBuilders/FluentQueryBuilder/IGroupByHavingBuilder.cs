using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more having clauses, which can still add more clauses to having
    /// </summary>
    public interface IGroupByHavingBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        /// <param name="having"></param>
        /// <returns></returns>
        IGroupByHavingBuilder<U, RB, R> Having(FormattableString having);

        /// <summary>
        /// Adds one column to orderby clauses.
        /// </summary>
        IOrderByBuilder<U, RB, R> OrderBy(FormattableString orderBy);

        /// <summary>
        /// Creates final query
        /// </summary>
        /// <returns></returns>
        R Build();
    }
}
