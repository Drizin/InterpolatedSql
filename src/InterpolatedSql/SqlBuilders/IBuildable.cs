namespace InterpolatedSql
{
    /// <summary>
    /// Classes implementing <see cref="IBuildable{R}"/> can build SQL statements and return type <typeparamref name="R"/>
    /// </summary>
    public interface IBuildable<out R>
        where R: class, IInterpolatedSql
    {
        /// <summary>
        /// Dynamically build SQL 
        /// </summary>
        R Build();
    }
}
