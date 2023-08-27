using System;
using System.Data;

namespace InterpolatedSql
{
    /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}"/>
    public interface IInterpolatedSqlBuilder<out U, out R> : IInterpolatedSqlBuilder, IBuildable<R>
        where U : IInterpolatedSqlBuilder<U, R>
        where R: class, IInterpolatedSql
    {
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AddParameter(SqlParameterInfo)"/>
        new U AddParameter(SqlParameterInfo parameterInfo);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AddParameter(string, object?, DbType?, ParameterDirection?, int?, byte?, byte?)"/>
        new U AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(IInterpolatedSql)"/>
        new U Append(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendFormattableString(FormattableString)"/>
        new U AppendFormattableString(FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendIf(bool, IInterpolatedSql)"/>
        new U AppendIf(bool condition, IInterpolatedSql value);


        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine()"/>
        new U AppendLine();

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine(IInterpolatedSql)"/>
        new U AppendLine(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLiteral(string)"/>
        new U AppendLiteral(string value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLiteral(string, int, int)"/>
        new U AppendLiteral(string value, int startIndex, int count);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendRaw(string)"/>
        new U AppendRaw(string value);


#if NET6_0_OR_GREATER
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(ref InterpolatedSqlHandler)"/>
        new U Append([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendIf(bool, ref InterpolatedSqlHandler)"/>
        new U AppendIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine(ref InterpolatedSqlHandler)"/>
        new U AppendLine(ref InterpolatedSqlHandler value);
#else
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(FormattableString)"/>
        new U Append(FormattableString value);
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendIf(bool, FormattableString)"/>
        new U AppendIf(bool condition, FormattableString value);
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine(FormattableString)"/>
        new U AppendLine(FormattableString value);
#endif

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Insert(int, IInterpolatedSql)"/>
        new U Insert(int index, IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Insert(int, FormattableString)"/>
        new U Insert(int index, FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.InsertLiteral(int, string)"/>
        new U InsertLiteral(int index, string value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Remove(int, int)"/>
        new U Remove(int startIndex, int length);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.TrimEnd"/>
        new U TrimEnd();

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Replace(string, IInterpolatedSql, out bool)"/>
        new U Replace(string oldValue, IInterpolatedSql newValue, out bool replaced);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Replace(string, IInterpolatedSql)"/>
        new U Replace(string oldValue, IInterpolatedSql newValue);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Replace(string, FormattableString, out bool)"/>
        new U Replace(string oldValue, FormattableString newValue, out bool replaced);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Replace(string, FormattableString)"/>
        new U Replace(string oldValue, FormattableString newValue);
    }
}