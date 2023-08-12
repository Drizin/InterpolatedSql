using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more having clauses, which can still add more clauses to having
    /// </summary>
    public interface IGroupByHavingBuilder<T>
        : ISqlCommand<T>

        where T : IFluentQueryBuilder<T>
    {

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        /// <param name="having"></param>
        /// <returns></returns>
        IGroupByHavingBuilder<T> Having(FormattableString having);

        /// <summary>
        /// Adds one column to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
