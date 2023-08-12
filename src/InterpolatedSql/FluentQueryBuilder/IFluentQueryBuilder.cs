namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// Any class that provides a fluent query builder.
    /// </summary>
    /// <typeparam name="T">The concrete type that implements this interface. Useful for Fluent APIs</typeparam>
    public interface IFluentQueryBuilder<T> :
        IEmptyQueryBuilder<T>,
        ISelectBuilder<T>,
        ISelectDistinctBuilder<T>,
        IFromBuilder<T>,
        IWhereBuilder<T>,
        IGroupByBuilder<T>,
        IGroupByHavingBuilder<T>,
        IOrderByBuilder<T>

        where T : IFluentQueryBuilder<T>
    {
    }
}