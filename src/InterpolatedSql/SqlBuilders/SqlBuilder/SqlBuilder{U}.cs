namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc/>
    /// <remarks>This is just a simplification of InterpolatedSqlBuilder{U, R} where R is IInterpolatedSql (if you don't need a custom return type)</remarks>
    public abstract class SqlBuilder<U> : SqlBuilder<U, IInterpolatedSql> // this is just a simplification of InterpolatedSqlBuilder<U, R> where R is IInterpolatedSql (if you don't need a custom return type)
        where U : SqlBuilder<U, IInterpolatedSql>, ISqlBuilder<U, IInterpolatedSql>
    {
    }
}