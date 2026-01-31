using InterpolatedSql.SqlBuilders;
using System;

namespace InterpolatedSql
{
    /// <summary>
    /// Parses FormattableString into <see cref="SqlBuilder"/> using regex.
    /// </summary>
    public interface IInterpolatedSqlParser
    {
        /// <summary>
        /// Parses a FormattableString and Appends it to an existing <see cref="SqlBuilder"/>
        /// </summary>
        void ParseAppend(IInterpolatedSqlBuilderBase target, FormattableString value);

        /// <summary>
        /// Parses a FormattableString and Inserts it at specified position into an existing <see cref="SqlBuilder"/>
        /// </summary>
        void ParseInsert(IInterpolatedSqlBuilderBase target, int index, FormattableString value);

        /// <summary>
        /// When a FormattableString is appended to an existing InterpolatedString, 
        /// the underlying format (where there are numeric placeholders) needs to be shifted because the arguments will have new positions in the final array
        /// This method is used to shift a format by a number of positions.
        /// </summary>
        string ShiftPlaceholderPositions(string format, Func<int, int> getNewPos);

        /// <inheritdoc cref="InterpolatedSqlParser.AdjustMultilineString"/>
        string AdjustMultilineString(string block);

        /// <summary>
        /// Transforms an argument based on format specifiers (e.g., ":text", ":varchar(100)").
        /// Handles conversion of types like XElement to string, and wraps values in StringParameterInfo or DbTypeParameterInfo when needed.
        /// </summary>
        void TransformArgument(ref object? argumentValue, ref int argumentAlignment, ref string? argumentFormat);
    }
}