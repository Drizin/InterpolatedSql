namespace InterpolatedSql.SqlBuilders.FluentQueryBuilder
{
    /// <summary>
    /// Any class that provides a fluent query builder.
    /// </summary>
    /// <typeparam name="U">The concrete type that implements this interface. Useful for Fluent APIs</typeparam>
    public interface IFluentQueryBuilder<out U, out RB, out R> :
        IEmptyQueryBuilder<U, RB, R>,
        ISelectBuilder<U, RB, R>,
        ISelectDistinctBuilder<U, RB, R>,
        IFromBuilder<U, RB, R>,
        IWhereBuilder<U, RB, R>,
        IGroupByBuilder<U, RB, R>,
        IGroupByHavingBuilder<U, RB, R>,
        IOrderByBuilder<U, RB, R>,
        ICompleteBuilder<U, RB, R>

        where U : IFluentQueryBuilder<U, RB, R>
        where RB : ISqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
    }
}