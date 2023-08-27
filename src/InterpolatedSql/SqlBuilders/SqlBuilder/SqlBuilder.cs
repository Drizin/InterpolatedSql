using System;
using System.Collections.Generic;
using System.Text;

namespace InterpolatedSql
{
    public interface ISqlBuilder : IInterpolatedSqlBuilder, IBuildable<IInterpolatedSql>
    {
    }

    /// <inheritdoc/>
    public class SqlBuilder : SqlBuilder<SqlBuilder>, ISqlBuilder
    {
        #region ctor
        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
            : base(options, format, arguments) 
        {
        }

        /// <inheritdoc />
        public SqlBuilder(InterpolatedSqlBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
        }


        /// <inheritdoc />
        public SqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be useful to provide Options (and Parser) immediately together
            if (value != null)
                Options.Parser.ParseAppend(this, value);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options)
            : base(literalLength, formattedCount, options)
        {
        }
        /// <inheritdoc />
        public SqlBuilder(int literalLength, int formattedCount) : this(literalLength, formattedCount, null)
        {
        }
#endif

        #endregion

    }

}