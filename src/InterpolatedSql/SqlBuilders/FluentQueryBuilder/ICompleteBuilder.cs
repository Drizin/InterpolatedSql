namespace InterpolatedSql.SqlBuilders.FluentQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more clause in where, which can still add more clauses to where
    /// </summary>
    public interface ICompleteBuilder<out U, out RB, out R>
        //: ISqlCommand<U>

        where U : IFluentQueryBuilder<U, RB, R>
        where RB : ISqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Builds the SQL statement
        /// </summary>
        R Build();
    }
}
