using System.Data;

namespace InterpolatedSql
{
    /// <summary>
    /// Any IInterpolatedSql that can be executed.
    /// Should contain DbConnection, Sql statement (complete and valid), and Parameters.
    /// Extension methods can be used to provide query/execution facades over this interface.
    /// Fluent Query Builders can use this interface to define which states are valid SQL statements.
    /// </summary>
    public interface ISqlCommand : IInterpolatedSql
    {
        /// <summary>
        /// Database connection associated to the command
        /// </summary>
        IDbConnection DbConnection { get; set; }
    }

    /// <inheritdoc cref="ISqlCommand" />
    /// <typeparam name="U">The underlying concrete type (or one of its interfaces) that implements this interface. Useful for Fluent APIs</typeparam>
    public interface ISqlCommand<out U> : ISqlCommand, IInterpolatedSql<U>
    {
    }

}
