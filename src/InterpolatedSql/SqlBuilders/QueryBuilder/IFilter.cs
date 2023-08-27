namespace InterpolatedSql
{
    /// <summary>
    /// Can be both individual filter or a list of filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Writes the filter(s) (both SQL Statement and SqlParameters) into an existing InterpolatedSqlBuilder
        /// </summary>
        void WriteTo(IInterpolatedSqlBuilderBase sb);

        /// <summary>
        /// If you're using Filters in standalone mode (without QueryBuilder), <br />
        /// you can just "build" the filters to get the string for the filters (with leading WHERE) and get the SqlParameters
        /// </summary>
        IInterpolatedSql Build();
    }
}
