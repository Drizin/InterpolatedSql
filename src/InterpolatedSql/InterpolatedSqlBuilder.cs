using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// InterpolatedSqlBuilder is a dynamic SQL builder (but injection safe) where SqlParameters are defined using string interpolation.
    /// Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
    /// and will be parametrized when you need to run the command.
    /// So it wraps the underlying SQL statement and the associated parameters, 
    /// allowing to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterpolatedSqlBuilder : InterpolatedSqlBuilder<InterpolatedSqlBuilder>, IInterpolatedSql
    {
        #region ctor
        /// <inheritdoc />
        public InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
            : base(options, format, arguments) 
        {
        }

        /// <inheritdoc />
        public InterpolatedSqlBuilder(InterpolatedSqlBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
            // Options can be defined in constructor but can also be set/modified after constructor (e.g. in initializer) - as long as it's set before parsing the first string
        }


        /// <inheritdoc />
        public InterpolatedSqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be useful to provide Options (and Parser) immediately together
            if (value != null)
                Options.Parser.ParseAppend(value, this);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedSqlBuilder(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
            : base(literalLength, formattedCount, options)
        {

        }
#endif

        #endregion

    }

}