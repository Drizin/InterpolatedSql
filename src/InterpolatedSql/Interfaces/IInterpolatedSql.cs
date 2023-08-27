using System;
using System.Collections.Generic;

namespace InterpolatedSql
{
    /// <summary>
    /// SQL Statement (text and parameters) written using string interpolation.
    /// </summary>
    public interface IInterpolatedSql
    {
        /// <summary>
        /// SQL Statement (text).
        /// The statement may refer to parameters (usually named like @p0, @p1, etc) that were captured using string interpolation - like "SELECT * FROM Table WHERE column >= @p0"
        /// Those associated parameters are stored in <see cref="SqlParameters"/>.
        /// It may (or may not) be a a complete and valid Sql statement (it can be a partial statement to be used/combined inside a more complex statement)
        /// </summary>
        string Sql { get; }

        /// <summary>
        /// This is the underlying format of the interpolated SQL statement.
        /// Exactly like <see cref="Sql"/>, but instead of rendering names to each parameter (like @p0, @p1, etc) this uses composite format 
        /// (<see cref="https://learn.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"/>,
        /// which means that the parameters (captured using string interpolation) are refered by numbered placeholders,
        /// like "SELECT * FROM Table WHERE column >= {0}".
        /// In other words, this is like <see cref="FormattableString.Format"/>.
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Parameters that were captured using string interpolation.
        /// E.g. if you create a statement like "SELECT * FROM Table WHERE column >= {variable}", then the first SqlParameter will be a reference to the variable.
        /// In other words, this is similar to <see cref="FormattableString.GetArguments"/>.
        /// <see cref="InterpolatedSqlParameter.Argument"/> Can be of any type (like int/string or any other primitive, or <see cref="SqlParameterInfo"/> or even System.Data.SqlParameter)
        /// </summary>
        IReadOnlyList<InterpolatedSqlParameter> SqlParameters { get; }

        /// <summary>
        /// Explicit parameters (named, and added explicitly - not using interpolated arguments)
        /// </summary>
        IReadOnlyList<SqlParameterInfo> ExplicitParameters { get; }
    }

    /// <inheritdoc cref="IInterpolatedSql" />
    /// Use this generic version if you have extensions depending on type <typeparamref name="U"/>
    /// <typeparam name="U">The underlying concrete type (or one of its interfaces) that implements this interface.</typeparam>
    public interface IInterpolatedSql<out U> : IInterpolatedSql
    {
    }

}
