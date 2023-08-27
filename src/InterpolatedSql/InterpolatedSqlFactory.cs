using System;

namespace InterpolatedSql
{
    /// <summary>
    /// Creates <see cref="SqlBuilder"/>
    /// </summary>
    public class InterpolatedSqlFactory
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual B Create<B>(ref InterpolatedSqlHandler<B> value)
            where B : IInterpolatedSqlBuilder
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (B)value.InterpolatedSqlBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual SqlBuilder Create(ref InterpolatedSqlHandler<SqlBuilder> value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (SqlBuilder)value.InterpolatedSqlBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual B Create<B>(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler<B> value)
            where B : IInterpolatedSqlBuilder
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (B) value.InterpolatedSqlBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual SqlBuilder Create(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler<SqlBuilder> value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return (SqlBuilder) value.InterpolatedSqlBuilder;
        }
#else
        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual B Create<B>(FormattableString source)
            where B : IInterpolatedSqlBuilder
        {
            B builder = InterpolatedSqlBuilderFactory.Default.Create<B>();
            Parser.ParseAppend(builder, source);
            return builder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual IInterpolatedSqlBuilder Create(FormattableString source)
        {
            var builder = InterpolatedSqlBuilderFactory.Default.Create();
            Parser.ParseAppend(builder, source);
            return builder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual B Create<B>(InterpolatedSqlBuilderOptions options, FormattableString source)
            where B : IInterpolatedSqlBuilder
        {
            B builder = InterpolatedSqlBuilderFactory.Default.Create<B>(options);
            Parser.ParseAppend(builder, source);
            return builder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual IInterpolatedSqlBuilder Create(InterpolatedSqlBuilderOptions options, FormattableString source)
        {
            var builder = InterpolatedSqlBuilderFactory.Default.Create(options);
            Parser.ParseAppend(builder, source);
            return builder;
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
