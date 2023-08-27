using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InterpolatedSql
{
    /// <summary>
    /// Parses FormattableString into <see cref="SqlBuilder"/>
    /// </summary>
    public class InterpolatedSqlParser : IInterpolatedSqlParser
    {
        #region statics/constants
        /// <summary>
        /// Regex to parse FormattableString
        /// </summary>
        protected static readonly Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(,(?<Alignment>(-)?\\d*))?(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        /// <summary>
        /// Identify all types of line-breaks
        /// </summary>
        protected static readonly Regex _lineBreaksRegex = new Regex(@"(\r\n|\n|\r)", RegexOptions.Compiled);

        /// <summary>
        /// String(maxlength) / nvarchar(maxlength) / String / nvarchar
        /// </summary>
        protected static readonly Regex regexDbTypeString = new Regex("^(String|nvarchar)\\s*(\\(\\s*(?<maxlength>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// StringFixedLength(length) / nchar(length) / StringFixedLength / nchar
        /// </summary>
        protected static readonly Regex regexDbTypeStringFixedLength = new Regex("^(StringFixedLength|nchar)\\s*(\\(\\s*(?<length>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// AnsiString(maxlength) / varchar(maxlength) / AnsiString / varchar
        /// </summary>
        protected static readonly Regex regexDbTypeAnsiString = new Regex("^(AnsiString|varchar)\\s*(\\(\\s*(?<maxlength>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// AnsiStringFixedLength(length) / char(length) / AnsiStringFixedLength / char
        /// </summary>
        protected static readonly Regex regexDbTypeAnsiStringFixedLength = new Regex("^(AnsiStringFixedLength|char)\\s*(\\(\\s*(?<length>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// text / varchar(MAX) / varchar(-1)
        /// </summary>
        protected static readonly Regex regexDbTypeText = new Regex("^(text|varchar\\s*(\\(\\s*((MAX|-1))\\s*\\)))$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// ntext / nvarchar(MAX) / nvarchar(-1)
        /// </summary>
        protected static readonly Regex regexDbTypeNText = new Regex("^(ntext|nvarchar\\s*(\\(\\s*((MAX|-1))\\s*\\)))$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        #endregion

        #region ctor
        /// <inheritdoc />
        public InterpolatedSqlParser()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses a FormattableStrings and Appends it into an existing InterpolatedSql
        /// </summary>
        public virtual void ParseAppend(IInterpolatedSqlBuilderBase target, FormattableString value)
        {
            ParseInsert(target, target.Format.Length, value);
        }

        /// <summary>
        /// Parses a FormattableStrings and Inserts it into an existing InterpolatedSql
        /// </summary>
        /// <param name="index">Position to insert</param>
        public virtual void ParseInsert(IInterpolatedSqlBuilderBase target, int index, FormattableString value)
        {
            if (value == null || string.IsNullOrEmpty(value.Format))
                return;
            bool append = (index == target.Format.Length);
            int currentIndex = index;
            object?[] arguments = value.GetArguments();
            if (arguments == null || arguments.Length == 0)
            {
                if (append)
                    AppendLiteral(target, value.Format);
                else
                    InsertLiteral(target, currentIndex, value.Format);
                return;
            }

            string format = value.Format;

            if (target.Options.AutoAdjustMultilineString)
                format = AdjustMultilineString(format);

            // Regex will find all placeholders, and iterate through the string processing the placeholders and the blocks before and after it
            // E.g. block, placeholder, block, placeholder, lastBlock
            var matches = _formattableArgumentRegex.Matches(format);
            int currentPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                int literalStart = currentPos; // previous pointer
                int literalEnd = matches[i].Index; // position of next placeholder 

                //string block = format.Substring(currentPos, matches[i].Index - currentPos);
                currentPos = matches[i].Index + matches[i].Length;

                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                var formatMatch = matches[i].Groups["Format"];
                var alignmentMatch = matches[i].Groups["Alignment"];
                string? argumentFormat = formatMatch.Success ? formatMatch.Value : null;
                int alignment = alignmentMatch.Success ? int.Parse(alignmentMatch.Value) : 0;
                object argument = arguments[argPos]!;

                if (append)
                {
                    AppendLiteral(target, format, literalStart, literalEnd - literalStart);
                    ProcessArgument(target, argument, alignment, argumentFormat);
                    target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
                }
                else
                {
                    InsertLiteral(target, currentIndex, format, literalStart, literalEnd - literalStart);
                    currentIndex += format.Length; // AppendArgument
                    var currentLength = target.Format.Length;
                    ProcessArgument(target, argument, alignment, argumentFormat);
                    target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
                    currentIndex += (target.Format.Length -currentLength);
                }
            }
            // Last literal
            if (append)
                AppendLiteral(target, format, currentPos, format.Length - currentPos);
            else
                InsertLiteral(target, currentIndex, format);

        }

        /// <summary>Appends a Literal</summary>
        protected virtual void AppendLiteral(IInterpolatedSqlBuilderBase target, string value, int startIndex, int count)
        {
            target.AppendLiteral(value, startIndex, count);
            if (count > 0)
                target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
        }

        /// <summary>Appends a Literal</summary>
        protected virtual void AppendLiteral(IInterpolatedSqlBuilderBase target, string value)
        {
            target.AppendLiteral(value);
            if (value.Any())
                target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
        }

        /// <summary>Inserts a Literal</summary>
        protected virtual void InsertLiteral(IInterpolatedSqlBuilderBase target, int index, string value)
        {
            target.InsertLiteral(index, value);
            if (value.Any())
                target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
        }

        /// <summary>Inserts a Literal</summary>
        protected virtual void InsertLiteral(IInterpolatedSqlBuilderBase target, int index, string value, int startIndex, int count)
        {
            target.InsertLiteral(index, value.Substring(startIndex, count));
            if (count > 0)
                target.AutoSpacing = false; // Autospacing should be applied only to beginning of an interpolated block appended programatically
        }


        /// <summary>
        /// Processes the embedded argument.
        /// </summary>
        protected virtual void ProcessArgument(IInterpolatedSqlBuilderBase target, object? argument, int argumentAlignment = 0, string? argumentFormat = null)
        {

            // InterpolatedSqlParameter is the argument of a previously parsed InterpolatedSqlBuilder
            if (argument is InterpolatedSqlParameter isArg && argumentFormat == null)
            {
                AppendArgument(target, isArg.Argument, 0, isArg.Format);
                return;
            }

            // If we get a nested InterpolatedSqlBuilder, it's already parsed - we just merge to current one
            if (argument is IInterpolatedSql isqlArg)
            {
                target.Append(isqlArg); // this will automatically shift the arguments
                return;
            }

            if (argument is InterpolatedSql.IBuildable iTransf)
            {
                target.Append(iTransf.Build()); // this will automatically shift the arguments
                return;
            }

            // If we get a nested FormattableString, we parse it (recursively) and merge it to current one
            if (argument is FormattableString fsArg)
            {
                var nestedStatement = InterpolatedSqlBuilderFactory.Default.Create();
                ParseAppend(nestedStatement, fsArg);
                target.Append(nestedStatement.Build()); // this will automatically shift the arguments
                return;
            }

            TransformArgument(ref argument, ref argumentAlignment, ref argumentFormat);
            AppendArgument(target, argument, argumentAlignment, argumentFormat);
        }

        /// <summary>
        /// This method can be used to transform the argument into different object types.
        /// 
        /// For most cases argumentValue will be a primitive type (int, string) and won't be transformed here, 
        /// then later in the process (when Dapper/ORM comes in) these primitive types are converted to your db types (for most cases can infer the right DbType).
        /// 
        /// For very specific cases it's possible to explicitly define the DbType using format specifiers,
        /// and in this case argumentValue will be transformed into a <see cref="StringParameterInfo"/> (for strings) or <see cref="DbTypeParameterInfo"/> (for other db types).
        /// </summary>
        protected virtual void TransformArgument(ref object? argumentValue, ref int argumentAlignment, ref string? argumentFormat)
        {
            var argFormats = argumentFormat?.Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList() ?? new List<string>();
            var direction = ParameterDirection.Input;
            DbType? dbType = null;
            DbType parsedDbType;
            Match m;

            // If argument is a string or IEnumerable<string> and argumentFormat is like "nvarchar(10)" we wrap the string under StringParameterInfo which brings additional info
            foreach (var testedFormat in argFormats.ToList())
            {
                bool matched = true;
                if (argumentValue is string && (m = regexDbTypeString.Match(testedFormat)) != null && m.Success) // String(maxlength) / nvarchar(maxlength) / String / nvarchar
                    argumentValue =
                        new StringParameterInfo()
                        {
                            IsAnsi = false,
                            IsFixedLength = false,
                            Value = (string)argumentValue,
                            Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(StringParameterInfo.DefaultLength, ((string)argumentValue).Length) : int.Parse(m.Groups["maxlength"].Value)),
                        };

                else if (argumentValue is string && (m = regexDbTypeAnsiString.Match(testedFormat)) != null && m.Success) // AnsiString(maxlength) / varchar(maxlength) / AnsiString / varchar
                    argumentValue = new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = false,
                        Value = (string)argumentValue,
                        Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(StringParameterInfo.DefaultLength, ((string)argumentValue).Length) : int.Parse(m.Groups["maxlength"].Value))
                    };

                else if (argumentValue is string && (m = regexDbTypeStringFixedLength.Match(testedFormat)) != null && m.Success) // StringFixedLength(length) / nchar(length) / StringFixedLength / nchar
                    argumentValue = new StringParameterInfo()
                    {
                        IsAnsi = false,
                        IsFixedLength = true,
                        Value = (string)argumentValue,
                        Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)argumentValue).Length : int.Parse(m.Groups["length"].Value))
                    };

                else if (argumentValue is string && (m = regexDbTypeAnsiStringFixedLength.Match(testedFormat)) != null && m.Success) // AnsiStringFixedLength(length) / char(length) / AnsiStringFixedLength / char
                    argumentValue = new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = true,
                        Value = (string)argumentValue,
                        Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)argumentValue).Length : int.Parse(m.Groups["length"].Value))
                    };

                else if (argumentValue is string && (m = regexDbTypeText.Match(testedFormat)) != null && m.Success) // text / varchar(MAX) / varchar(-1)
                    argumentValue = new StringParameterInfo()
                    {
                        IsAnsi = false,
                        IsFixedLength = true,
                        Value = (string)argumentValue,
                        Length = int.MaxValue
                    };

                else if (argumentValue is string && (m = regexDbTypeNText.Match(testedFormat)) != null && m.Success) // ntext / nvarchar(MAX) / nvarchar(-1)
                    argumentValue = new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = true,
                        Value = (string)argumentValue,
                        Length = int.MaxValue
                    };

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeString.Match(testedFormat)) != null && m.Success) // String(maxlength) / nvarchar(maxlength) / String / nvarchar
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = false,
                        IsFixedLength = false,
                        Value = str,
                        Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(StringParameterInfo.DefaultLength, ((string)str).Length) : int.Parse(m.Groups["maxlength"].Value))
                    });

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeAnsiString.Match(testedFormat)) != null && m.Success) // AnsiString(maxlength) / varchar(maxlength) / AnsiString / varchar
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = false,
                        Value = str,
                        Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(StringParameterInfo.DefaultLength, ((string)str).Length) : int.Parse(m.Groups["maxlength"].Value))
                    });

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeStringFixedLength.Match(testedFormat)) != null && m.Success) // StringFixedLength(length) / nchar(length) / StringFixedLength / nchar
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = false,
                        IsFixedLength = true,
                        Value = str,
                        Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)str).Length : int.Parse(m.Groups["length"].Value))
                    });

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeAnsiStringFixedLength.Match(testedFormat)) != null && m.Success) // AnsiStringFixedLength(length) / char(length) / AnsiStringFixedLength / char
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = true,
                        Value = str,
                        Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)str).Length : int.Parse(m.Groups["length"].Value))
                    });

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeText.Match(testedFormat)) != null && m.Success) // text / varchar(MAX) / varchar(-1)
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = false,
                        IsFixedLength = true,
                        Value = str,
                        Length = int.MaxValue
                    });

                else if (argumentValue is IEnumerable<string> && (m = regexDbTypeNText.Match(testedFormat)) != null && m.Success) // ntext / nvarchar(MAX) / nvarchar(-1)
                    argumentValue = ((IEnumerable<string>)argumentValue).Select(str => new StringParameterInfo()
                    {
                        IsAnsi = true,
                        IsFixedLength = true,
                        Value = str,
                        Length = int.MaxValue
                    });

                else if (dbType == null && Enum.TryParse<System.Data.DbType>(value: testedFormat, ignoreCase: true, result: out parsedDbType))
                {
                    argumentValue = new DbTypeParameterInfo(null, argumentValue, direction, parsedDbType);
                }

                //TODO: did this ever work? Output parameters embedded directly in interpolated string?
                //I guess we would need to interpolate a custom type wrapping at least an explicit name, plus optional lambda setter (or else the value would be stored in the parameter itself)
                //else if (testedFormat == "out")
                //{
                //    direction = ParameterDirection.Output;
                //}

                else
                {
                    matched = false;
                }

                if (matched)
                {
                    argFormats.RemoveAll(a => a == testedFormat);
                }

                //TODO: parse SqlDbTypes?
                // https://stackoverflow.com/questions/35745226/net-system-type-to-sqldbtype
                // https://gist.github.com/tecmaverick/858392/53ddaaa6418b943fa3a230eac49a9efe05c2d0ba
            }

            if (!string.IsNullOrEmpty(argumentFormat))
                argumentFormat = string.Join(",", argFormats);
        }

        /// <summary>
        /// Appends an interpolated argument.
        /// </summary>
        protected virtual void AppendArgument(IInterpolatedSqlBuilderBase target, object? argument, int argumentAlignment, string? argumentFormat)
        {
            // Appends the {argPos} (e.g. "{2}"), and save the object into target.SqlParameters
            target.AppendArgument(argument, argumentAlignment, argumentFormat);
        }

        /// <summary>
        /// When an interpolated string is appended to another, the placeholder positions must be shifted.
        /// </summary>
        public string ShiftPlaceholderPositions(string format, Func<int, int> getNewPos)
        {
            string newFormat = _formattableArgumentRegex.Replace(format.ToString(), match => ReplacePlaceholderPosition(match, getNewPos));
            return newFormat; //TODO: use StringBuilder and do replaces inline, saving memory
        }

        /// <summary>
        /// When a FormattableString is appended to an existing InterpolatedString, 
        /// the underlying format (where there are numeric placeholders) needs to be shifted because the arguments will have new positions in the final array
        /// </summary>
        protected virtual string ReplacePlaceholderPosition(Match match, Func<int, int> getNewPos)
        {
            Group parm = match.Groups[4];
            int previousPos = int.Parse(parm.Value);
            int newPos = getNewPos(previousPos);
            string replace = newPos.ToString();
            string newPlaceholder = string.Format("{0}{1}{2}", match.Value.Substring(0, parm.Index - match.Index), replace, match.Value.Substring(parm.Index - match.Index + parm.Length));
            return newPlaceholder;
        }

        // Multi-line blocks can be conveniently used with any indentation, and we will correctly adjust the indentation of those blocks (TrimLeftPadding and TrimFirstEmptyLine)
        /// <summary>
        /// Given a text block (multiple lines), this removes the left padding of the block, by calculating the minimum number of spaces which happens in EVERY line.
        /// Then, other methods writes the lines one by one, which in case will respect the current indent of the writer.
        /// This is legacy (backwards compatibility), but new code should just use C# Raw String Literals which will do the same (remove block indentation and first empty line)
        /// </summary>
        public string AdjustMultilineString(string block)
        {
            // copied from https://github.com/CodegenCS/CodegenCS/

            string[] parts = _lineBreaksRegex.Split(block);
            if (parts.Length <= 1) // no linebreaks at all
                return block;
            var nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            if (nonEmptyLines.Count <= 1) // if there's not at least 2 non-empty lines, assume that we don't need to adjust anything
                return block;

            Match m = _lineBreaksRegex.Match(block);
            if (m != null && m.Success && m.Index == 0)
            {
                block = block.Substring(m.Length); // remove first empty line
                parts = _lineBreaksRegex.Split(block);
                nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            }


            int minNumberOfSpaces = nonEmptyLines.Select(nonEmptyLine => nonEmptyLine.Length - nonEmptyLine.TrimStart().Length).Min();

            StringBuilder sb = new StringBuilder();

            var matches = _lineBreaksRegex.Matches(block);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = block.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = block.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                sb.Append(line.Substring(Math.Min(line.Length, minNumberOfSpaces)));
                sb.Append(lineBreak);
            }
            string lastLine = block.Substring(lastPos);
            sb.Append(lastLine.Substring(Math.Min(lastLine.Length, minNumberOfSpaces)));

            return sb.ToString();
        }
        #endregion

    }
}