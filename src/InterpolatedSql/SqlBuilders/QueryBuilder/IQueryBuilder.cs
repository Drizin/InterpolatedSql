﻿using System;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc cref="QueryBuilder{U, RB, R}"/>
    public interface IQueryBuilder<U, RB, R> : IBuildable<R>
        where U : IQueryBuilder<U, RB, R>, IBuildable<R>
        where RB : IInterpolatedSqlBuilderBase, IBuildable<R>
        where R : class, IInterpolatedSql
    {
        Filters.FiltersType FiltersType { get; set; }

        IInterpolatedSqlBuilderBase? GetFilters();
        U Where(Filter filter);
        U Where(Filters filters);
        U Where(FormattableString filter);
#if NET6_0_OR_GREATER
        U From(ref InterpolatedSqlHandler fromString);
        U GroupBy(ref InterpolatedSqlHandler selectString);
        U Having(ref InterpolatedSqlHandler selectString);
        U OrderBy(ref InterpolatedSqlHandler selectString);
        U Select(ref InterpolatedSqlHandler selectString);
#else
        U From(FormattableString fromString);
        U GroupBy(FormattableString selectString);
        U Having(FormattableString selectString);
        U OrderBy(FormattableString selectString);
        U Select(FormattableString selectString);
#endif
    }
}