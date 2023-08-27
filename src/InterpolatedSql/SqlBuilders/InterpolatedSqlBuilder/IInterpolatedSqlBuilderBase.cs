using System;
using System.Data;

namespace InterpolatedSql
{
    /// <inheritdoc cref="InterpolatedSqlBuilderBase"/>
    public interface IInterpolatedSqlBuilderBase: IBuildable
    {
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AutoSpacing"/>
        bool AutoSpacing { get; set; }

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Format"/>
        string Format { get; }

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.IsEmpty"/>
        bool IsEmpty { get; }

        /// <inheritdoc cref="InterpolatedSqlBuilderOptions"/>
        InterpolatedSqlBuilderOptions Options { get; }

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AddObjectProperties(object)"/>
        void AddObjectProperties(object obj);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AddParameter(SqlParameterInfo)"/>
        void AddParameter(SqlParameterInfo parameterInfo);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AddParameter(string, object?, DbType?, ParameterDirection?, int?, byte?, byte?)"/>
        void AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(IInterpolatedSql)"/>
        void Append(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendArgument(object?, int, string?)"/>
        void AppendArgument(object? argument, int alignment = 0, string? format = null);


        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendFormattableString(FormattableString)"/>
        void AppendFormattableString(FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendIf(bool, IInterpolatedSql)"/>
        void AppendIf(bool condition, IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendLine"/>
        void AppendLine();

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendLine(IInterpolatedSql)"/>
        void AppendLine(IInterpolatedSql value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLiteral(string)"/>
        void AppendLiteral(string value);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendLiteral(string, int, int)"/>
        void AppendLiteral(string value, int startIndex, int count);
        
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AppendRaw(string)"/>
        void AppendRaw(string value);
#if NET6_0_OR_GREATER
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(ref InterpolatedSqlHandler)"/>
        void Append([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendIf(bool, ref InterpolatedSqlHandler)"/>
        void AppendIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine(ref InterpolatedSqlHandler)"/>
        void AppendLine([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value);
#else
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Append(FormattableString)"/>
        void Append(FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendIf(bool, FormattableString)"/>
        void AppendIf(bool condition, FormattableString value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.AppendLine(FormattableString)"/>
        void AppendLine(FormattableString value);
#endif


        /// <inheritdoc cref="InterpolatedSqlBuilderBase.AsFormattableString"/>
        FormattableString AsFormattableString();

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.IndexOf(string, bool)"/>
        int IndexOf(string value, bool ignoreCase = false);

        
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.IndexOf(string, int, bool)"/>
        int IndexOf(string value, int startIndex, bool ignoreCase);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Insert(int, FormattableString)"/>
        void Insert(int index, FormattableString value);
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Insert(int, IInterpolatedSql)"/>
        void Insert(int index, IInterpolatedSql value);
        
        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.InsertLiteral(int, string)"/>
        void InsertLiteral(int index, string value);

        /// <inheritdoc cref="InterpolatedSqlBuilder{U, R}.Remove(int, int)"/>
        void Remove(int startIndex, int length);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Replace(string, FormattableString)"/>
        void Replace(string oldValue, FormattableString newValue);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Replace(string, FormattableString, out bool)"/>
        void Replace(string oldValue, FormattableString newValue, out bool replaced);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Replace(string, IInterpolatedSql)"/>
        void Replace(string oldValue, IInterpolatedSql newValue);

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.Replace(string, IInterpolatedSql, out bool)"/>
        void Replace(string oldValue, IInterpolatedSql newValue, out bool replaced);

       
        /// <inheritdoc cref="InterpolatedSqlBuilderBase.ToString"/>
        string ToString();

        /// <inheritdoc cref="InterpolatedSqlBuilderBase.TrimEnd"/>
        void TrimEnd();
    }
}