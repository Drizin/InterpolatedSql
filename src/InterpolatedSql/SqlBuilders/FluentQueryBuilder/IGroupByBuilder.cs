using System;

namespace InterpolatedSql.SqlBuilders.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more groupby clauses, which can still add more clauses to groupby
    /// </summary>
    public interface IGroupByBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : ISqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds one or more column(s) to groupby clauses.
        /// </summary>
        IGroupByBuilder<U, RB, R> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds one or more condition(s) to having clauses.
        /// </summary>
        IGroupByHavingBuilder<U, RB, R> Having(FormattableString having);

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        IOrderByBuilder<U, RB, R> OrderBy(FormattableString orderBy);

        /// <summary>
        /// Creates final query
        /// </summary>
        /// <returns></returns>
        R Build();
    }
}
