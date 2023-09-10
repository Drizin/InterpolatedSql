using System;

namespace InterpolatedSql.SqlBuilders.FluentQueryBuilder
{
    /// <summary>
    /// QueryBuilder which is preparing a SELECT DISTINCT statement
    /// </summary>
    public interface ISelectDistinctBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : ISqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<U, RB, R> SelectDistinct(FormattableString select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<U, RB, R> SelectDistinct(params FormattableString[] moreColumns);


        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<U, RB, R> From(FormattableString from);
    }
}
