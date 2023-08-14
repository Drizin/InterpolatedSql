namespace InterpolatedSql.Dapper.FluentQueryBuilder
{
    /// <summary>
    /// Extensions for Dapper FluentQueryBuilder
    /// </summary>
    public static partial class FluentQueryBuilderExtensions
    {
        /// <summary>
        /// Any Interpolated Sql (even if not complete) that is being built by a FluentQueryBuilder (and is hidden by the Fluent interfaces) can be cast to the concrete type.
        /// </summary>
        public static T AsDapperFluentQueryBuilder<T>(this IInterpolatedSql<T> statement)
            where T : FluentQueryBuilder<T>
        {
            return (T)statement;
        }

        /// <summary>
        /// A SqlCommand that is considered "complete and valid" (by the Fluent API builder) should be Built() before it's executed.
        /// </summary>
        public static IDapperSqlCommand Build<T>(this ISqlCommand<T> statement)
            where T : FluentQueryBuilder
        {
            return (IDapperSqlCommand)(FluentQueryBuilder)statement;
        }
    }
}
