namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Global Options
    /// </summary>
    public class InterpolatedSqlDapperOptions
    {
        /// <inheritdoc />
        public InterpolatedSqlDapperOptions()
        {
        }

        /// <summary>
        /// Responsible for parsing SqlParameters (see <see cref="IInterpolatedSql.SqlParameters"/>) 
        /// into a list of SqlParameterInfo that 
        /// </summary>
        public SqlParameterMapper InterpolatedSqlParameterParser { get; set; } = SqlParameterMapper.DefaultMapper;

        /// <summary>
        /// When SqlBuilder constructor doesn't specify options, will use these Default options
        /// </summary>
        public static InterpolatedSqlDapperOptions DefaultOptions { get; set; } = new InterpolatedSqlDapperOptions();

        /// <summary>
        /// Creates a new instance copying all values from <see cref="DefaultOptions"/>
        /// </summary>
        public static InterpolatedSqlDapperOptions Create()
        {
            return new InterpolatedSqlDapperOptions()
            {
                InterpolatedSqlParameterParser = SqlParameterMapper.DefaultMapper
            };
        }

    }
}
