using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// Dynamic SQL builder where SqlParameters are defined using string interpolation (but it's injection safe). This is the most important piece of the library.
    ///
    /// Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
    /// and will be parametrized when you need to run the command.
    /// So it wraps the underlying SQL statement and the associated parameters, 
    /// allowing to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    /// <typeparam name="U">Recursive Generics: U Should be the same class that implements InterpolatedSqlBuilder{U}, or any other interface implemented by this class</typeparam>
    /// <typeparam name="R">R Should be the type that this builder builds (type returned by Build()) - should implement IInterpolatedSql</typeparam>
    /// <remarks>Fluent Builder with Recursive Generics - allows Fluent API to always return the same type U</remarks>
    public abstract class InterpolatedSqlBuilder<U, R> : InterpolatedSqlBuilderBase, IInterpolatedSqlBuilder<U, R>, IBuildable
        where U :  IInterpolatedSqlBuilder<U, R>
        where R : class, IInterpolatedSql
    {

        #region Static Members
        /// <summary>
        /// Default options used when options is not defined in constructor.
        /// Note that since this a generic class (with generic types <typeparamref name="U"/> and <typeparamref name="R"/>) this static member will be different for different U/R types.
        /// So if you're using a subclass (like InterpolatedSql.Dapper.SqlBuilder or InterpolatedSql.Dapper.QueryBuilder) you should define defaults specifically for that subclass.
        /// </summary>
        public static InterpolatedSqlBuilderOptions DefaultOptions { get; set; } = new InterpolatedSqlBuilderOptions();
        #endregion

        #region Members
        /// <summary>
        /// Object bag - can be used in custom extensions (but consider creating a subclass instead of using this)
        /// </summary>
        public Dictionary<string, object> ObjectBag => _objectBag ??= new Dictionary<string, object>();

        private Dictionary<string, object>? _objectBag = null;
        #endregion

        #region ctor
        /// <inheritdoc />
        protected InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options ?? DefaultOptions, format, arguments)
        {
        }

        /// <inheritdoc />
        public InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
        }


        /// <inheritdoc />
        public InterpolatedSqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be important to provide Options (and Parser) immediately together
            if (value != null)
            {
                Options.Parser.ParseAppend(this, value);
                ResetAutoSpacing(); // rearm after appending initial text
            }
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedSqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options ?? DefaultOptions)
        {
        }
#endif
        #endregion

        #region Methods
        #endregion

        #region Fluent Builder (all methods here should be public and return same type T)

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
#else
        /// <inheritdoc/>
        public new U Append(FormattableString value)
        {
            base.Append(value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendIf(bool condition, FormattableString value)
        {
            base.AppendIf(condition, value);
            return (U)(object)this;
        }

        /// <inheritdoc/>
        public new U AppendLine(FormattableString value)
        {
            base.AppendLine(value);
            return (U)(object)this;
        }
#endif

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
        public static U operator +(InterpolatedSqlBuilder<U, R> interpolatedString, FormattableString value)
        {
            interpolatedString.AppendFormattableString(value);
            interpolatedString.ResetAutoSpacing();
            return (U)(object)interpolatedString;
        }

        /// <inheritdoc cref="AppendFormattableString(FormattableString)"/>
        public static U operator +(InterpolatedSqlBuilder<U, R> interpolatedString, IInterpolatedSql value)
        {
            interpolatedString.Append(value);
            interpolatedString.ResetAutoSpacing();
            return (U)(object)interpolatedString;
        }

        /// <summary>
        /// For "bare" builders (like <see cref="SqlBuilder"/> or <see cref="SqlBuilder{T}"/>) this just returns the same instance,
        /// but casted (through a wrapper) to <see cref="IInterpolatedSql"/> so you 
        /// For "enricher" builders (classes that implement extend <see cref="IBuildable"/> and will transform the final sql statement
        /// like <see cref="QueryBuilder"/> , <see cref="QueryBuilder{U, RB, R}"/>, <see cref="FluentQueryBuilder.FluentQueryBuilder{U, RB, R}"/>
        /// then the builder will be "transformed" (usually creating a new instance which is the "enriched"/combined sql statement).
        /// </summary>
        public abstract R Build();

        /// <inheritdoc cref="IBuildable.Build"/>
        IInterpolatedSql IBuildable.Build() 
        {
            // Redirect IBuildable.Build() to the new abstract R Build()
            return Build(); 
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
#else
        void IInterpolatedSqlBuilderBase.Append(FormattableString value)
        {
            Append(value);
        }

        void IInterpolatedSqlBuilderBase.AppendIf(bool condition, FormattableString value)
        {
            AppendIf(condition, value);
        }

        void IInterpolatedSqlBuilderBase.AppendLine(FormattableString value)
        {
            AppendLine(value);
        }
#endif
        #endregion

    }
}