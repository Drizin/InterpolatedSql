﻿using System;
using System.Collections.Generic;

namespace InterpolatedSql
{
    /// <summary>
    /// Settings for InterpolatedSqlBuilder
    /// </summary>
    public class InterpolatedSqlBuilderOptions
    {
        #region Members
        /// <summary>
        /// All <see cref="InterpolatedSqlBuilder"/> methods that take a <see cref="FormattableString"/> will use this Parser to parse the FormattableString.
        /// By default it's <see cref="InterpolatedSqlParser"/>
        /// </summary>
        public IInterpolatedSqlParser Parser { get; set; } = new InterpolatedSqlParser();

        /// <summary>
        /// If true (default is true) then all curly braces are escaped (with double curly braces).
        /// By default <see cref="System.FormattableString.Format"/> uses indexed placeholders (numbered placeholders like "{0}", "{1}", etc.) to indicate the arguments,
        /// and so does <see cref="InterpolatedSqlBuilderBase.Format"/>.
        /// If your derived type does NOT need those indexed placeholders (e.g. you're writing your own format and do not need the standard one) or
        /// if your derived type will never write curly braces through <see cref="InterpolatedSqlBuilderBase.AppendLiteral(string)"/>, 
        /// then you can disable this (set to false) so that it won't escape curly braces (and it will be faster).
        /// You can also use <see cref="InterpolatedSqlBuilderBase.AppendRaw(string)"/>) which does not escape anything.
        /// </summary>
        internal bool AutoEscapeCurlyBraces { get; set; } = true;

        /// <summary>
        /// Argument Formatting (what comes after colon, like $"My string {val:000}") is always extracted into <see cref="InterpolatedSqlParameter.Format"/>.
        /// If this is true (default is true) then this formatting will also be preserved in the underlying <see cref="InterpolatedSqlBuilderBase.Format"/>
        /// (else, <see cref="InterpolatedSqlBuilderBase.Format"/> will only have the numeric placeholder like {0} but without the extracted formatting).
        /// </summary>
        internal bool PreserveArgumentFormatting { get; set; } = false;

        /// <summary>
        /// If true (default is false) each added parameter will check if identical parameter (same type and value)
        /// was already added, and if so will reuse the existing parameter.
        /// </summary>
        public bool ReuseIdenticalParameters { get; set; } = false;

        /// <summary>
        /// Compares two arguments for reusing. Only used if <see cref="ReuseIdenticalParameters"/> is true.
        /// By default it's <see cref="InterpolatedSqlParameterComparer"/>
        /// </summary>
        public IEqualityComparer<InterpolatedSqlParameter> ArgumentComparer { get; set; } = new InterpolatedSqlParameterComparer();

        /// <summary>
        /// In the rendered SQL statement the parameters by default are named like @p0, @p1, etc. <br />
        /// You can change the name p0/p1/etc to any other prfix. <br />
        /// Example: if you set to "arg" you'll get @arg0, @arg1, etc. <br />
        /// </summary>
        public string AutoGeneratedParameterPrefix { get; set; } = "p";

        /// <summary>
        /// String that is appended to the parameter name for enumerable types to avoid name conflicts.
        /// </summary>
        public string ParameterArrayNameSuffix { get; set; } = "array";

        /// <summary>
        /// In the rendered SQL statement the parameters by default are named like @p0, @p1, etc. <br />
        /// If your database does not accept @ symbol you can change for any other symbol. <br />
        /// For Oracle you should use ":" <br />
        /// </summary>
        public string DatabaseParameterSymbol { get; set; } = "@";


        /// <summary>
        /// If true (default is true), single quotes surrounding interpolated arguments will be automatically stripped
        /// </summary>
        public bool AutoFixSingleQuotes { get; set; } = true;

        /// <summary>
        /// If true (default is true), then spaces will be automatically added if there's no whitespace (or linebreak) between last text and the new text.
        /// This assume that a single SQL statement (or identifier, or value) will always be appended in a single statement (why would anyone split a single sql word in 2 appends, right?)
        /// </summary>
        public bool AutoSpace { get; set; } = true;


        /// <summary>
        /// This is for legacy compatibility - it adjusts blocks similarly to what is currently available with C#11 Raw String Literals
        /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/raw-string-literal" />
        /// </summary>
        public bool AutoAdjustMultilineString { get; set; } = false;


        /// <summary>
        /// Given a parameter name, how it should be formatted when the SQL statement is generated (e.g. add "@" before name for MSSQL, or add ":" for Oracle, etc.)
        /// </summary>
        public Func<string, string> FormatParameterName { get; set; }

        /// <summary>
        /// Given the argument (object) and it's index position (int), this defines how the auto generated parameter name should be named (e.g. add "p" before position)
        /// </summary>
        public Func<InterpolatedSqlParameter, int, string> CalculateAutoParameterName { get; set; }
        #endregion

        /// <inheritdoc/>
        public InterpolatedSqlBuilderOptions()
        {
            FormatParameterName = (parameterName) => DatabaseParameterSymbol + parameterName;
            CalculateAutoParameterName = (parameter, pos) => AutoGeneratedParameterPrefix + pos.ToString();
        }

    }
}
