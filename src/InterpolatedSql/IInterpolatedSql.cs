using System;
using System.Collections.Generic;

namespace InterpolatedSql
{
    /// <summary>
    /// Sql statement written using string interpolation.
    /// </summary>
    public interface IInterpolatedSql
    {
        /// <summary>
        /// Like <see cref="FormattableString.Format"/> - it's a string containing numbered placeholders - like "SELECT * FROM Table WHERE column >= {0}".
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Like <see cref="FormattableString.GetArguments"/> - it's the values embedded using string interpolation.
        /// E.g. if you create a statement using "SELECT * FROM Table WHERE column >= {variable}", then variable will be the first SqlParameter.
        /// <see cref="InterpolatedSqlParameter.Argument"/> Can be of any type (like int/string or any other primitive, or <see cref="SqlParameterInfo"/> or even System.Data.SqlParameter)
        /// </summary>
        IReadOnlyList<InterpolatedSqlParameter> SqlParameters { get; }

        /// <summary>
        /// Explicit parameters (named, and added explicitly - not using interpolated arguments)
        /// </summary>
        IReadOnlyList<SqlParameterInfo> ExplicitParameters { get; }

        /// <inheritdoc cref="InterpolatedSqlBuilderOptions"/>
        public InterpolatedSqlBuilderOptions Options { get; set; }
    }

}
