using System.Data;

namespace InterpolatedSql
{
    /// <summary>
    /// Any IInterpolatedSql that can be executed.
    /// Should contain DbConnection, Sql statement (complete and valid), and Parameters.
    /// Fluent Query Builders can use this interface to define which states are valid SQL statements,
    /// and extension methods can be used to provide query/execution facades over this interface
    /// </summary>
    public interface ISqlCommand : IInterpolatedSql
    {
        /// <summary>
        /// Database connection associated to the command
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// Sql statement of the command
        /// </summary>
        string Sql { get; }
    }

    /// <inheritdoc />
    /// <typeparam name="T">The concrete type that implements this interface. Useful for Fluent APIs</typeparam>
    public interface ISqlCommand<T> : ISqlCommand, IInterpolatedSql<T>
    {
    }

}
