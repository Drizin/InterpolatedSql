using System;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more clause in where, which can still add more clauses to where
    /// </summary>
    public interface ICompleteBuilder<T>
        : ISqlCommand<T>

        where T : IFluentQueryBuilder<T>
    {
    }
}
