using System;
using System.Data;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc cref="SqlBuilder{U, R}"/>
    public interface ISqlBuilder<out U, out R> : IInterpolatedSqlBuilderBase, IBuildable<R>
        where U : ISqlBuilder<U, R>
        where R: class, IInterpolatedSql
    {
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AddParameter(SqlParameterInfo)"/>
        new U AddParameter(SqlParameterInfo parameterInfo);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AddParameter(string, object?, DbType?, ParameterDirection?, int?, byte?, byte?)"/>
        new U AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Append(IInterpolatedSql)"/>
        new U Append(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendFormattableString(FormattableString)"/>
        new U AppendFormattableString(FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendIf(bool, IInterpolatedSql)"/>
        new U AppendIf(bool condition, IInterpolatedSql value);


        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLine()"/>
        new U AppendLine();

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLine(IInterpolatedSql)"/>
        new U AppendLine(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLiteral(string)"/>
        new U AppendLiteral(string value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLiteral(string, int, int)"/>
        new U AppendLiteral(string value, int startIndex, int count);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendRaw(string)"/>
        new U AppendRaw(string value);


#if NET6_0_OR_GREATER
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Append(ref InterpolatedSqlHandler)"/>
        new U Append([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendIf(bool, ref InterpolatedSqlHandler)"/>
        new U AppendIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLine(ref InterpolatedSqlHandler)"/>
        new U AppendLine(ref InterpolatedSqlHandler value);

        #region Overloads to disambiguate with FormattableString overloads - NET6+ users will probably prefer the InterpolatedStringHandler overload, but sometimes will need this one (e.g. F# can't use InterpolatedStringHandler)
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Append(FormattableString, object?)"/>
        new U Append(FormattableString value, object? dummy = null);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendIf(bool, FormattableString, object?)"/>
        new U AppendIf(bool condition, FormattableString value, object? dummy = null);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLine(FormattableString, object?)"/>
        new U AppendLine(FormattableString value, object? dummy = null);
        #endregion
#else
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Append(FormattableString)"/>
        new U Append(FormattableString value);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendIf(bool, FormattableString)"/>
        new U AppendIf(bool condition, FormattableString value);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.AppendLine(FormattableString)"/>
        new U AppendLine(FormattableString value);
#endif

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Insert(int, IInterpolatedSql)"/>
        new U Insert(int index, IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Insert(int, FormattableString)"/>
        new U Insert(int index, FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.InsertLiteral(int, string)"/>
        new U InsertLiteral(int index, string value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Remove(int, int)"/>
        new U Remove(int startIndex, int length);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.TrimEnd"/>
        new U TrimEnd();

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Replace(string, IInterpolatedSql, out bool)"/>
        new U Replace(string oldValue, IInterpolatedSql newValue, out bool replaced);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Replace(string, IInterpolatedSql)"/>
        new U Replace(string oldValue, IInterpolatedSql newValue);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Replace(string, FormattableString, out bool)"/>
        new U Replace(string oldValue, FormattableString newValue, out bool replaced);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase{U, R}.Replace(string, FormattableString)"/>
        new U Replace(string oldValue, FormattableString newValue);
    }
}