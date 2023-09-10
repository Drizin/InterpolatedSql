using System;

namespace InterpolatedSql.SqlBuilders
{
    /// <summary>
    /// Generic factory to create any builder (subclasses of <see cref="IInterpolatedSqlBuilderBase"/>).
    /// This uses reflection and is for advanced customization scenarios, for most cases you should just use <see cref="SqlBuilderFactory"/>.
    /// </summary>
    public class InterpolatedSqlBuilderFactory<B>
            where B : IInterpolatedSqlBuilderBase, new()
    {
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create()
        {
            var ctor = typeof(B).GetConstructor(new Type[] { });
            var builder = (B)ctor.Invoke(new object[] { });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create(InterpolatedSqlBuilderOptions options)
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(InterpolatedSqlBuilderOptions) });
            B builder;
            if (ctor != null)
            {
                builder = (B)ctor.Invoke(new object[] { options });
            }
            else
            {
                ctor = typeof(B).GetConstructor(new Type[] { });
                builder = (B)ctor.Invoke(new object[] { });
                builder.Options = options;
            }
            return builder;
        }


#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create(int literalLength, int formattedCount)
        {
            B builder;
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(int), typeof(int) });
            if (ctor != null)
            {
                builder = (B)ctor.Invoke(new object[] { literalLength, formattedCount });
            }
            else
            {
                ctor = typeof(B).GetConstructor(new Type[] { });
                builder = (B)ctor.Invoke(new object[] { });
            }
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions options)
        {
            B builder;
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(InterpolatedSqlBuilderOptions) });
            if (ctor != null)
            {
                builder = (B)ctor.Invoke(new object?[] { literalLength, formattedCount, options });
            }
            else
            {
                ctor = typeof(B).GetConstructor(new Type[] { });
                builder = (B)ctor.Invoke(new object[] { });
                builder.Options = options;
            }
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create(ref InterpolatedSqlHandler<B> value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return value.InterpolatedSqlBuilder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create(InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler<B> value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return value.InterpolatedSqlBuilder;
        }

#else

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create(FormattableString value)
        {
            B builder;
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(FormattableString) });
            if (ctor != null)
            {
                builder = (B)ctor.Invoke(new object[] { value });
            }
            else
            {
                ctor = typeof(B).GetConstructor(new Type[] { });
                builder = (B)ctor.Invoke(new object[] { });
                Parser.ParseAppend(builder, value);
            }
            return builder;
        }

        /// <summary>
        /// Creates a new InterpolatedSqlBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual B Create(InterpolatedSqlBuilderOptions options, FormattableString value)
        {
            B builder = Create(options);
            Parser.ParseAppend(builder, value);
            return builder;
        }

#endif


        /// <summary>
        /// Default Factory
        /// </summary>
        public static InterpolatedSqlBuilderFactory<B> Default = new InterpolatedSqlBuilderFactory<B>();

        /// <summary>
        /// Default Parser
        /// </summary>
        public InterpolatedSqlParser Parser = new InterpolatedSqlParser();

    }
}
