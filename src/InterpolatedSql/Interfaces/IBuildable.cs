namespace InterpolatedSql
{
    /// <summary>
    /// Classes implementing <see cref="IBuildable"/> can build SQL statements, 
    /// either by just exposing the underlying properties of a SQL Builder (anything inheriting <see cref="IInterpolatedSqlBuilder{U, R}"/>),
    /// or by applying custom logic (enriching/augmenting on top of a SQL builder).
    /// For the second case (enriching logic) there's usually an underlying SqlBuilder which is stores an initial template, and then there are some commands where users can fill the template blanks.
    /// </summary>
    public interface IBuildable<out R> : IBuildable
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Dynamically build SQL 
        /// </summary>
        new R Build();
    }

    /// <inheritdoc cref="IBuildable{R}"/>
    public interface IBuildable
    {
        /// <summary>
        /// Dynamically build SQL 
        /// </summary>
        IInterpolatedSql Build();
    }
}
