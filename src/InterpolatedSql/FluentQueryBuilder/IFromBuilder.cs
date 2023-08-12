using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more from clauses, which can still add more clauses to from
    /// </summary>
    public interface IFromBuilder<T>
        : ISqlCommand<T>

        where T : IFluentQueryBuilder<T>
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        ISelectBuilder<T> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<T> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<T> From(FormattableString from);

        /// <summary>
        /// Adds a new group of conditions to where clauses.
        /// </summary>
        IWhereBuilder<T> Where(Filter filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<T> Where(Filters filter);

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        IWhereBuilder<T> Where(FormattableString filter);

        /// <summary>
        /// Adds a new condition to groupby clauses.
        /// </summary>
        IGroupByBuilder<T> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
