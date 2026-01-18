using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc />
    public abstract class InterpolatedSqlBuilderBase<U, R> : InterpolatedSqlBuilderBase, IBuildable<R>, IInterpolatedSqlBuilderBase
        where R : class, IInterpolatedSql
    {
        #region Members
        /// <summary>
        /// Object bag - can be used in custom extensions (but consider creating a subclass instead of using this)
        /// </summary>
        public Dictionary<string, object> ObjectBag => _objectBag ??= new Dictionary<string, object>();

        private Dictionary<string, object>? _objectBag = null;
        #endregion

        #region ctors

        /// <inheritdoc />
        protected InterpolatedSqlBuilderBase(InterpolatedSqlBuilderOptions? options, StringBuilder? format, IEnumerable<InterpolatedSqlParameter>? arguments) : base(options, format, arguments)
        {
        }
#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedSqlBuilderBase(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options)
        {
        }
#endif
        #endregion

        #region Fluent Builder (all methods here should be public and return same type U)

        /// <inheritdoc/>
        public new U AppendLine()
        {
            base.AppendLine();
            return (U)(object)this;
        }


        /// <inheritdoc/>
        public new U TrimEnd()
        {
            base.TrimEnd();
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Append(IInterpolatedSql value)
        {
            base.Append(value);
            return (U)(object)this;
        }


        /// <inheritdoc/>
        public new U AppendIf(bool condition, IInterpolatedSql value)
        {
            base.AppendIf(condition, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLine(IInterpolatedSql value)
        {
            base.AppendLine(value);
            return (U)(object)this;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public new U Append([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            base.Append(ref value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value)
        {
            base.AppendIf(condition, ref value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLine(ref InterpolatedSqlHandler value)
        {
            base.AppendLine(ref value);
            return (U)(object)this;
        }
#endif
        /// <inheritdoc/>
        public new U Append(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            base.Append(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendIf(bool condition, FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            base.AppendIf(condition, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLine(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            base.AppendLine(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLiteral(string value)
        {
            base.AppendLiteral(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLiteral(string value, int startIndex, int count)
        {
            base.AppendLiteral(value, startIndex, count);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendFormattableString(FormattableString value)
        {
            base.AppendFormattableString(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendRaw(string value)
        {
            base.AppendRaw(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Insert(int index, IInterpolatedSql value)
        {
            base.Insert(index, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Insert(int index, FormattableString value)
        {
            base.Insert(index, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U InsertLiteral(int index, string value)
        {
            base.InsertLiteral(index, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Remove(int startIndex, int length)
        {
            base.Remove(startIndex, length);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Replace(string oldValue, IInterpolatedSql newValue)
        {
            base.Replace(oldValue, newValue);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Replace(string oldValue, IInterpolatedSql newValue, out bool replaced)
        {
            base.Replace(oldValue, newValue, out replaced);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Replace(string oldValue, FormattableString newValue)
        {
            base.Replace(oldValue, newValue);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U Replace(string oldValue, FormattableString newValue, out bool replaced)
        {
            base.Replace(oldValue, newValue, out replaced);
            return (U)(object)this;
        }

        #endregion

        #region Explicit Adding Parameters
        /// <inheritdoc/>
        public virtual new U AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            base.AddParameter(parameterName, parameterValue, dbType, direction, size, precision, scale);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AddParameter(SqlParameterInfo parameterInfo)
        {
            base.AddParameter(parameterInfo);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public virtual new U AddObjectProperties(object obj)
        {
            base.AddObjectProperties(obj);
            return (U)(object)this;
        }
        #endregion

        #region Conversions

        /// <inheritdoc cref="AppendFormattableString(FormattableString)"/>
        public static U operator +(InterpolatedSqlBuilderBase<U, R> interpolatedString, FormattableString value)
        {
            interpolatedString.AppendFormattableString(value);
            interpolatedString.ResetAutoSpacing();
            return (U)(object)interpolatedString;
        }

        /// <inheritdoc cref="AppendFormattableString(FormattableString)"/>
        public static U operator +(InterpolatedSqlBuilderBase<U, R> interpolatedString, IInterpolatedSql value)
        {
            interpolatedString.Append(value);
            interpolatedString.ResetAutoSpacing();
            return (U)(object)interpolatedString;
        }

        void IInterpolatedSqlBuilderBase.AppendLiteral(string value)
        {
            AppendLiteral(value);
        }

        void IInterpolatedSqlBuilderBase.Append(IInterpolatedSql value)
        {
            Append(value);
        }

        void IInterpolatedSqlBuilderBase.InsertLiteral(int index, string value)
        {
            InsertLiteral(index, value);
        }

        void IInterpolatedSqlBuilderBase.Remove(int startIndex, int length)
        {
            Remove(startIndex, length);
        }

        void IInterpolatedSqlBuilderBase.AppendFormattableString(FormattableString value)
        {
            AppendFormattableString(value);
        }

#if NET6_0_OR_GREATER
        void IInterpolatedSqlBuilderBase.Append([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            Append(ref value);
        }

        void IInterpolatedSqlBuilderBase.AppendIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value)
        {
            AppendIf(condition, ref value);
        }

        void IInterpolatedSqlBuilderBase.AppendLine([System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            AppendLine(ref value);
        }
#endif
        void IInterpolatedSqlBuilderBase.Append(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            Append(value);
        }

        void IInterpolatedSqlBuilderBase.AppendIf(bool condition, FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            AppendIf(condition, value);
        }

        void IInterpolatedSqlBuilderBase.AppendLine(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            AppendLine(value);
        }
        #endregion



        /// <inheritdoc cref="IBuildable{R}.Build"/>
        public abstract R Build();
    }
}
