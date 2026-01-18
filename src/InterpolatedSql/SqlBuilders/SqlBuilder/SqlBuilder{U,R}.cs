using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.SqlBuilders
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
    public abstract class SqlBuilder<U, R> : InterpolatedSqlBuilderBase<U, R>, ISqlBuilder<U, R>, IBuildable<R>
        where U :  ISqlBuilder<U, R>
        where R : class, IInterpolatedSql
    {
        #region ctor
        /// <inheritdoc />
        protected SqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, IEnumerable<InterpolatedSqlParameter>? arguments) : base(options ?? InterpolatedSqlBuilderOptions.DefaultOptions, format, arguments)
        {
        }

        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
        }


        /// <inheritdoc />
        public SqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(options: options)
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
        public SqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options ?? InterpolatedSqlBuilderOptions.DefaultOptions)
        {
        }
#endif
        #endregion


    }
}