using InterpolatedSql.SqlBuilders;

namespace InterpolatedSql
{
    /// <summary>
    /// Classes implementing <see cref="ISqlEnricher"/> can enrich/augment on top of a SQL builder (or build a whole new statament) applying custom logic
    /// Usually those classes have an underlying SqlBuilder which is stores an initial template, and then there are some commands where users can fill the template blanks,
    /// and the <see cref="GetEnrichedSql"/> will combine all inputs returning a single Sql.
    /// </summary>
    public interface ISqlEnricher
    {
        /// <summary>
        /// Dynamically build SQL 
        /// </summary>
        IInterpolatedSql GetEnrichedSql();
    }
}
