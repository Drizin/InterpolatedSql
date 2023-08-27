using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more clause in where, which can still add more clauses to where
    /// </summary>
    public interface IWhereBuilder<out U, out RB, out R>
        where U : IFluentQueryBuilder<U, RB, R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<U, RB, R> Where(Filter filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<U, RB, R> Where(Filters filter);

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        IWhereBuilder<U, RB, R> Where(FormattableString filter);

        /// <summary>
        /// Adds a new condition to groupby clauses.
        /// </summary>
        IGroupByBuilder<U, RB, R> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<U, RB, R> OrderBy(FormattableString orderBy);

        /// <summary>
        /// Creates final query
        /// </summary>
        /// <returns></returns>
        R Build();
    }
}
