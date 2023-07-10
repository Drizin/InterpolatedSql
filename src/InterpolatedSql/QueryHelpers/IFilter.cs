namespace InterpolatedSql
{
    /// <summary>
    /// Can be both individual filter or a list of filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Writes the SQL Statement of the filter
        /// </summary>
        void WriteTo(InterpolatedSqlBuilderBase sb);
    }
}
