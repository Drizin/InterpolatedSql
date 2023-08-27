using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Empty QueryBuilder (initialized without a template), which can start both with Select() or SelectDistinct()
    /// </summary>
    public interface IEmptyQueryBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelectBuilder<U, RB, R> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<U, RB, R> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<U, RB, R> SelectDistinct(FormattableString select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<U, RB, R> SelectDistinct(params FormattableString[] moreColumns);
    }
}
