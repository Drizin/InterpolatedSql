using System;
using System.Collections.Generic;
using System.Text;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more groupby clauses, which can still add more clauses to groupby
    /// </summary>
    public interface IGroupByBuilder<T>
        : ISqlCommand<T>

        where T : IFluentQueryBuilder<T>
    {
        /// <summary>
        /// Adds one or more column(s) to groupby clauses.
        /// </summary>
        IGroupByBuilder<T> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds one or more condition(s) to having clauses.
        /// </summary>
        IGroupByHavingBuilder<T> Having(FormattableString having);

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
