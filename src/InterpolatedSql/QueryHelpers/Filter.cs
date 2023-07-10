﻿using System;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// Filter statement defined in a single statement <br />
    /// It can include multiple conditions (if defined in a single statement during constructor), <br />
    /// but usually this is used as one condition (one column, one comparison operator, and one parameter).
    /// </summary>
    public class Filter : InterpolatedSqlBuilder, IFilter
    {
        /// <summary>
        /// New Filter statement. <br />
        /// Example: $"[CategoryId] = {categoryId}" <br />
        /// Example: $"[Name] LIKE {productName}"
        /// </summary>
        public Filter(FormattableString filter) : base(filter)
        {
        }

        /// <inheritdoc/>
        public void WriteTo(InterpolatedSqlBuilderBase builder)
        {
            builder.Append(this);
        }
    }
}
