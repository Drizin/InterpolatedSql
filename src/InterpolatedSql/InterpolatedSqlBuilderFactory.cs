using System;

namespace InterpolatedSql
{
    /// <summary>
    /// Creates <see cref="IInterpolatedSqlBuilder"/>
    /// </summary>
    public class InterpolatedSqlBuilderFactory
    {
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create<B>()
            where B : IInterpolatedSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { });
            B builder = (B)ctor.Invoke(new object[] { });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create<B>(InterpolatedSqlBuilderOptions options)
            where B : IInterpolatedSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(InterpolatedSqlBuilderOptions) });
            B builder = (B)ctor.Invoke(new object[] { options });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create<B>(int literalLength, int formattedCount)
            where B : IInterpolatedSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(int), typeof(int) });
            B builder = (B)ctor.Invoke(new object[] { literalLength, formattedCount });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create<B>(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
            where B : IInterpolatedSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(InterpolatedSqlBuilderOptions) });
            B builder = (B)ctor.Invoke(new object[] { literalLength, formattedCount, options });
            return builder;
        }


        /// <summary>
        /// Creates the default IInterpolatedSqlBuilder, which by default is SqlBuilder
        /// </summary>
        public virtual IInterpolatedSqlBuilder Create(InterpolatedSqlBuilderOptions? options = null)
        {
            IInterpolatedSqlBuilder builder = new SqlBuilder(options);
            return builder;
        }

        /// <summary>
        /// Creates the default IInterpolatedSqlBuilder, which by default is SqlBuilder
        /// </summary>
        public virtual IInterpolatedSqlBuilder Create(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
        {
            IInterpolatedSqlBuilder builder = new SqlBuilder(options); //TODO: use literalLength, formattedCount
            return builder;
        }


        /// <summary>
        /// Default Factory
        /// </summary>
        public static InterpolatedSqlBuilderFactory Default = new InterpolatedSqlBuilderFactory();
    }
}
