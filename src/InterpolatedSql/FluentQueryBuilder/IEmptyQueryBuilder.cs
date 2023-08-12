using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Empty QueryBuilder (initialized without a template), which can start both with Select() or SelectDistinct()
    /// </summary>
    public interface IEmptyQueryBuilder<T>
        where T : IFluentQueryBuilder<T>
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelectBuilder<T> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<T> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<T> SelectDistinct(FormattableString select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<T> SelectDistinct(params FormattableString[] moreColumns);
    }
}
