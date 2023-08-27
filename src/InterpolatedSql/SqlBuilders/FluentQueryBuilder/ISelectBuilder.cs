using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder which is preparing a SELECT statement
    /// </summary>
    public interface ISelectBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        ISelectBuilder<U, RB, R> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<U, RB, R> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<U, RB, R> From(FormattableString from);
    }
}
