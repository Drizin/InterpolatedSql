namespace InterpolatedSql
{
    /// <inheritdoc/>
    /// <remarks>This is just a simplification of InterpolatedSqlBuilder{U, R} where R is IInterpolatedSql (if you don't need a custom return type)</remarks>
    public abstract class InterpolatedSqlBuilder<U> : InterpolatedSqlBuilder<U, IInterpolatedSql> // this is just a simplification of InterpolatedSqlBuilder<U, R> where R is IInterpolatedSql (if you don't need a custom return type)
        where U : InterpolatedSqlBuilder<U, IInterpolatedSql>, IInterpolatedSqlBuilder<U, IInterpolatedSql>
    {
        /// <inheritdoc/>
        public override IInterpolatedSql Build()
        {
            return this.AsSql();
        }
    }
}