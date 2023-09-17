namespace InterpolatedSql.SqlBuilders.InsertUpdateBuilder
{
    /// <inheritdoc/>
    public class InsertUpdateBuilder : InsertUpdateBuilder<InsertUpdateBuilder, ISqlBuilder<SqlBuilder, IInterpolatedSql>, IInterpolatedSql>
    {
        /// <inheritdoc/>
        public InsertUpdateBuilder(string tableName) : base(tableName, opts => new SqlBuilder(opts))
        {
        }
        
    }

}
