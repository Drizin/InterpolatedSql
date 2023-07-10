using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace InterpolatedSql
{
    /// <inheritdoc cref="InterpolatedSqlBuilderBase"/>
    /// <typeparam name="T">T Should be the same class that implements InterpolatedSqlBuilder{T}.</typeparam>
    /// <remarks>Fluent Builder with Recursive Generics - allows Fluent API to always return the same type T</remarks>
    public class InterpolatedSqlBuilder<T> : InterpolatedSqlBuilderBase
        where T : InterpolatedSqlBuilder<T>
    {

        #region ctor
        /// <inheritdoc />
        protected InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments) { }

        /// <inheritdoc />
        public InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options = null) : base(options) { }


        /// <inheritdoc />
        public InterpolatedSqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : base(value, options) { }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedSqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options)
        {
        }
#endif


        #endregion

        // All public methods that return parent type should be here

        #region Fluent Builder

        /// <inheritdoc/>
        public new T Append(LegacyFormattableString value)
        {
            base.Append(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T Append(IInterpolatedSql value)
        {
            base.Append(value);
            return (T)this;
        }


        /// <inheritdoc/>
        public new T AppendIf(bool condition, IInterpolatedSql value)
        {
            base.AppendIf(condition, value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendLine(IInterpolatedSql value)
        {
            base.AppendLine(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendIf(bool condition, LegacyFormattableString value)
        {
            base.AppendIf(condition, value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendLine(LegacyFormattableString value)
        {
            base.AppendLine(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendLine()
        {
            base.AppendLine();
            return (T)this;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc/>        
        public new T Append([InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            base.Append(ref value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendIf(bool condition, [InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value)
        {
            base.AppendIf(condition, ref value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendLine(ref InterpolatedSqlHandler value)
        {
            base.AppendLine(ref value);
            return (T)this;
        }
#endif

        /// <inheritdoc/>
        public new T AppendLiteral(string value)
        {
            base.AppendLiteral(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendLiteral(string value, int startIndex, int count)
        {
            base.AppendLiteral(value, startIndex, count);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendFormattableString(FormattableString value)
        {
            base.AppendFormattableString(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AppendRaw(string value)
        {
            base.AppendRaw(value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AddParameter(SqlParameterInfo parameterInfo)
        {
            base.AddParameter(parameterInfo);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            base.AddParameter(parameterName, parameterValue, dbType, direction, size, precision, scale);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T AddObjectProperties(object obj)
        {
            base.AddObjectProperties(obj);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T Insert(int index, IInterpolatedSql value)
        {
            base.Insert(index, value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T Insert(int index, LegacyFormattableString value)
        {
            base.Insert(index, value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T InsertLiteral(int index, string value)
        {
            base.InsertLiteral(index, value);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T Remove(int startIndex, int length)
        {
            base.Remove(startIndex, length);
            return (T)this;
        }
        
        /// <inheritdoc/>
        public new T Replace(string oldValue, IInterpolatedSql newValue)
        {
            base.Replace(oldValue, newValue);
            return (T)this;
        }
                
        /// <inheritdoc/>
        public T Replace(string oldValue, IInterpolatedSql newValue, out bool replaced)
        {
            replaced = base.Replace(oldValue, newValue);
            return (T)this;
        }

        /// <inheritdoc/>
        public new T Replace(string oldValue, LegacyFormattableString newValue)
        {
            base.Replace(oldValue, newValue);
            return (T)this;
        }

        /// <inheritdoc/>
        public T Replace(string oldValue, LegacyFormattableString newValue, out bool replaced)
        {
            replaced = base.Replace(oldValue, newValue);
            return (T)this;
        }

        #endregion


        /// <inheritdoc cref="AppendFormattableString(FormattableString)"/>
        public static T operator +(InterpolatedSqlBuilder<T> interpolatedString, FormattableString value)
        {
            interpolatedString.AppendFormattableString(value);
            return (T) interpolatedString;
        }

        /// <inheritdoc cref="AppendFormattableString(FormattableString)"/>
        public static T operator +(InterpolatedSqlBuilder<T> interpolatedString, IInterpolatedSql value)
        {
            interpolatedString.Append(value);
            return (T)interpolatedString;
        }
    }
}