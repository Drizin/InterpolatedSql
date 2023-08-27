using System.Collections.Generic;
using System.Text;
using System;

namespace InterpolatedSql
{
    /// <inheritdoc/>
    /// <remarks>This is just a simplification of InterpolatedSqlBuilder{U, R} where R is IInterpolatedSql (if you don't need a custom return type) 
    /// and U is this same type (if you dont'need to extend the class)</remarks>
    public class InterpolatedSqlBuilder : InterpolatedSqlBuilder<InterpolatedSqlBuilder, IInterpolatedSql>
    {
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

        /// <inheritdoc/>
        public override IInterpolatedSql Build()
        {
            return this.AsSql();
        }
    }
}