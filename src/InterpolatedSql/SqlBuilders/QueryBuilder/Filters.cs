using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InterpolatedSql.SqlBuilders
{
    /// <summary>
    /// Multiple Filter statements which are grouped together. Can be grouped with ANDs or ORs.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Filters : List<IFilter>, IFilter
    {
        #region Members
        /// <summary>
        /// By default Filter Groups are combined with AND operator. But you can use OR.
        /// </summary>
        public FiltersType Type { get; set; } = FiltersType.AND;

        /// <summary>
        /// How a list of Filters are combined (AND operator or OR operator)
        /// </summary>
        public enum FiltersType
        {
            /// <summary>
            /// AND
            /// </summary>
            AND,

            /// <summary>
            /// OR
            /// </summary>
            OR
        }
        #endregion

        #region ctor

        /// <summary>
        /// Create a new group of filters.
        /// </summary>
        public Filters(FiltersType type, IEnumerable<IFilter> filters)
        {
            Type = type;
            this.AddRange(filters);
        }

        /// <summary>
        /// Create a new group of filters which are combined with AND operator.
        /// </summary>
        public Filters(IEnumerable<IFilter> filters): this(FiltersType.AND, filters)
        {
        }

        /// <summary>
        /// Create a new group of filters from formattable strings
        /// </summary>
        public Filters(FiltersType type, params FormattableString[] filters):
            this(type, filters.Select(fiString => new Filter(fiString)))
        {
        }

        /// <summary>
        /// Create a new group of filters from formattable strings which are combined with AND operator.
        /// </summary>
        public Filters(params FormattableString[] filters) : this(FiltersType.AND, filters)
        {
        }
        #endregion

        #region IFilter
        /// <inheritdoc/>
        public void WriteTo(IInterpolatedSqlBuilderBase sb)
        {
            //if (this.Count() > 1) //TODO: EncloseMultipleConditions to wrap first-level conditions around parentheses
            //    sb.Append("(");
            for (int i = 0; i < this.Count(); i++)
            {
                if (i > 0 && Type == FiltersType.AND)
                    sb.AppendLiteral(" AND ");
                else if (i > 0 && Type == FiltersType.OR)
                    sb.AppendLiteral(" OR ");
                IFilter filter = this[i];
                if (filter is Filters && ((Filters)filter).Count() > 1) // only put brackets in groups after the first level
                {
                    sb.AppendLiteral("(");
                    filter.WriteTo(sb);
                    sb.AppendLiteral(")");
                }
                else
                    filter.WriteTo(sb);
            }
            //if (this.Count() > 1)
            //    sb.Append(")");
        }

        /// <inheritdoc/>
        public IInterpolatedSql Build()
        {
            var command = SqlBuilderFactory.Default.Create();
            WriteTo(command);
            if (!command.IsEmpty && !command.Format.StartsWith("WHERE "))
                command.InsertLiteral(0, "WHERE ");
            return command.Build();
        }

        private string DebuggerDisplay 
        {
            get 
            {
                var sb = SqlBuilderFactory.Default.Create();
                sb.AppendLiteral("(");
                sb.AppendLiteral(this.Count().ToString());
                sb.AppendLiteral(" filters): "); 
                WriteTo(sb); 
                return sb.ToString(); 
            } 
        }
        #endregion

    }
}
