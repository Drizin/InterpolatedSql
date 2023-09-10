using System.Collections.Generic;
using System.Text;
using System;

namespace InterpolatedSql.SqlBuilders
{
    /// <inheritdoc/>
    /// <remarks>This is just a simplification of InterpolatedSqlBuilder{U, R} where R is IInterpolatedSql (if you don't need a custom return type) 
    /// and U is this same type (if you dont'need to extend the class)</remarks>
    public class SqlBuilder : SqlBuilder<SqlBuilder, IInterpolatedSql>
    {
        #region ctor
        /// <inheritdoc />
        protected internal SqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options ?? InterpolatedSqlBuilderOptions.DefaultOptions, format, arguments)
        {
        }

        /// <inheritdoc />
        public SqlBuilder() : this(options: null, format: null, arguments: null)
        {
        }

        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions options) : this(options: options, format: null, arguments: null)
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

        /// <inheritdoc />
        public override IInterpolatedSql Build()
        {
            return AsSql();
        }

    }
}