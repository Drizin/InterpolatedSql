using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace InterpolatedSql
{
    /// <summary>
    /// Compares two sql parameters for reusing duplicates. Only used if <see cref="InterpolatedSqlBuilderOptions.ReuseIdenticalParameters"/> is true.
    /// </summary>
    public class InterpolatedSqlParameterComparer : IEqualityComparer<InterpolatedSqlParameter>
    {
        /// <inheritdoc/>
        public bool Equals(InterpolatedSqlParameter? arg1, InterpolatedSqlParameter? arg2)
        {
            if (arg1 == null && arg2 == null)
                return true;
            if (arg1 == null || arg2 == null)
                return false;
            if (arg1.Format != arg2.Format)
                return false;
            if (arg1.Argument == null && arg2.Argument == null)
                return true;
            if (arg1.Argument == null || arg2.Argument == null)
                return false;
            if (arg1.Argument.GetType() == arg2.Argument.GetType() && arg1.Argument.Equals(arg2.Argument))
                return true;
            return false;
        }

        /// <inheritdoc/>
        public int GetHashCode(
#if NETCOREAPP
            [DisallowNull] 
#endif
        InterpolatedSqlParameter obj)
        {
            return (obj.Argument?.GetHashCode() ?? 0) ^ (obj.Format?.GetHashCode() ?? 0);
        }
    }
}
