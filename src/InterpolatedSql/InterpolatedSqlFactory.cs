using System;

namespace InterpolatedSql
{
    /// <summary>
    /// Creates <see cref="InterpolatedSqlBuilder"/>
    /// </summary>
    public class InterpolatedSqlFactory
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual InterpolatedSqlBuilder Create(ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (InterpolatedSqlBuilder)value.InterpolatedSqlBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual InterpolatedSqlBuilder Create(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (InterpolatedSqlBuilder) value.InterpolatedSqlBuilder;
        }
#else
        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual InterpolatedSqlBuilder Create(FormattableString source)
        {
            var target = new InterpolatedSqlBuilder();
            Parser.ParseAppend(source, target);
            return target;
        }
#endif



        /// <summary>
        /// Default Parser
        /// </summary>
        public InterpolatedSqlParser Parser = new InterpolatedSqlParser();

        /// <summary>
        /// Default Factory
        /// </summary>
        public static InterpolatedSqlFactory Default = new InterpolatedSqlFactory();
    }
}
