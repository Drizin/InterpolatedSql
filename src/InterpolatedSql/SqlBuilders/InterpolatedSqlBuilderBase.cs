using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace InterpolatedSql.SqlBuilders
{
    /// <summary>
    /// Dynamic SQL builder where SqlParameters are defined using string interpolation (but it's injection safe). This is the most important piece of the library.
    ///
    /// Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
    /// and will be parametrized when you need to run the command.
    /// So it wraps the underlying SQL statement and the associated parameters, 
    /// allowing to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class InterpolatedSqlBuilderBase : IInterpolatedSqlBuilderBase
    {
        #region Members
        /// <inheritdoc cref="InterpolatedSqlBuilderOptions"/>
        public InterpolatedSqlBuilderOptions Options { get; set; }

        /// <summary>
        /// If next write should automatically add a spacing separator (if required) to isolate it from the previous text.
        /// Depends on <see cref="Options"/> (<see cref="InterpolatedSqlBuilderOptions.AutoSpacingOptions"/> not null), and will be reset (rearmed) after each call to public append methods.
        /// </summary>
        public bool AutoSpacing { get; set; }

        /// <summary>
        /// Resets AutoSpacing to original value.(rearms), invoked after calls to public append methods.
        /// </summary>
        protected void ResetAutoSpacing()
        {
            AutoSpacing = Options.AutoSpacingOptions != null;
        }

        /// <summary>
        /// This is like <see cref="FormattableString.Format"/> which is the underlying format of the interpolated string.
        /// </summary>
        public string Format => _format.ToString();

        /// <inheritdoc cref="Format" />
        protected readonly StringBuilder _format;

        /// <summary>
        /// Sql Parameters that were embedded using string interpolation (actually they can be any object type) 
        /// </summary>
        protected readonly List<InterpolatedSqlParameter> _sqlParameters;

        /// <summary>
        /// Sql Parameters that were embedded using string interpolation (actually they can be any object type) 
        /// </summary>
        protected IReadOnlyList<InterpolatedSqlParameter> SqlParameters => _sqlParameters;

        /// <summary>
        /// Explicit parameters (named, and added explicitly - not using interpolated arguments)
        /// </summary>
        protected readonly List<SqlParameterInfo> _explicitParameters = new List<SqlParameterInfo>();

        /// <summary>
        /// Explicit Parameters
        /// </summary>
        protected IReadOnlyList<SqlParameterInfo> ExplicitParameters => _explicitParameters;


        /// <summary>
        /// Optional associated DbConnection. 
        /// <see cref="SqlBuilder{U, R}"/> will not enforce this value, but if defined it can be used by custom extensions.
        /// (but instead of using this nullable property please consider creating a subclass and hiding it with a non-nullable property - see InterpolatedSql.Dapper.SqlBuilder)
        /// </summary>
        public IDbConnection? DbConnection { get; set; }

        /// <summary>
        /// By default this is <see cref="Environment.NewLine"/>
        /// </summary>
        protected string NewLine { get; set; } = Environment.NewLine;

        private int _lastLiteralEnd = -1;             // position where last literal char was written
        private int _lastLiteralArgumentsCount = 0;   // how many arguments were there when the last literal was added

        /// <summary>
        /// SQL Statement (text)
        /// </summary>
        protected string Sql => BuildSql(Format, _sqlParameters);

        /// <summary>
        /// Bulds SQL statement
        /// </summary>
        /// <param name="format"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        protected string BuildSql(string format, IEnumerable<InterpolatedSqlParameter> sqlParameters)
        {
            if (sqlParameters.Any())
                return string.Format(format, sqlParameters.Select((parm, pos) => Options.FormatParameterName(CalculateAutoParameterName(parm, pos))).ToArray());
            else
                return format;
        }

        /// <summary>
        /// Checks if there were no literals and no SqlParameters added
        /// </summary>
        public bool IsEmpty => _format.Length == 0 && !_sqlParameters.Any();
        #endregion

        #region ctors

        /// <inheritdoc />
        protected InterpolatedSqlBuilderBase(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
        {
            Options = options ?? new InterpolatedSqlBuilderOptions();
            ResetAutoSpacing();
            _format = format ?? new StringBuilder();
            _sqlParameters = arguments ?? new List<InterpolatedSqlParameter>();
        }
#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedSqlBuilderBase(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
        {
            Options = options ?? new InterpolatedSqlBuilderOptions();
            ResetAutoSpacing();
            _format = new StringBuilder(literalLength + formattedCount * 4); // embedded parameter will usually occupy 3-4 chars ("{0}"..."{99}")
            _sqlParameters = new List<InterpolatedSqlParameter>(formattedCount);
        }
#endif
        #endregion

        #region Methods
        /// <summary>
        /// Appends an argument and returns the index of the argument in the list <see cref="SqlParameters"/>.
        /// This method will only add the argument object to the list of arguments - it will NOT add the placeholder to the format StringBuilder (e.g. "{2}").
        /// Appending parameters and adding them to the underlying StringBuilder is usually done by concatenating or (equivalent) by calling <see cref="AppendArgument(object, int, string)"/>.
        /// If <see cref="InterpolatedSqlBuilderOptions.ReuseIdenticalParameters"/> is true then this method will try to reuse existing parameters, according to <see cref="InterpolatedSqlBuilderOptions.ArgumentComparer"/>
        /// </summary>
        /// <returns>Position where parameter was added</returns>
        protected internal virtual int AddArgument(InterpolatedSqlParameter argument)
        {
            if (Options.ReuseIdenticalParameters && Options.ArgumentComparer != null)
                // Reuse existing parameters (don't pass duplicates)
                for (int i = 0; i < _sqlParameters.Count; i++)
                    if (Options.ArgumentComparer.Equals(_sqlParameters[i], argument))
                        return i;

            _sqlParameters.Add(argument);
            return _sqlParameters.Count - 1;
        }

        /// <summary>
        /// Appends an argument.
        /// This will both add the argument object to the list of arguments (see <see cref="AddArgument(InterpolatedSqlParameter)"/>)
        /// and will also add the placeholder to the format StringBuilder (e.g. "{2}")
        /// </summary>
        public virtual void AppendArgument(object? argument, int alignment = 0, string? format = null)
        {
            if (format == "raw")
            {
                AppendRaw(argument?.ToString() ?? "");
                return;
            }

            // Transform argument based on format specifier before storing it
            // This ensures both FormattableString (legacy) and InterpolatedStringHandler (NET6+) paths
            // get the full transformation logic (string types, DbTypes, XElement handling, etc.)
            Options.Parser.TransformArgument(ref argument, ref alignment, ref format);

            int argumentPos = AddArgument(new InterpolatedSqlParameter(argument, format));
            if (AutoSpacing)
                CheckAutoSpacing(null);
            _format.Append("{");
            _format.Append(argumentPos);
            if (alignment != 0)
                _format.Append("," + alignment.ToString());
            if (Options.PreserveArgumentFormatting && !string.IsNullOrEmpty(format))
                _format.Append(":").Append(format);
            _format.Append("}");
        }

        /// <summary>
        /// Appends to this instance another InterpolatedString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public void Append(IInterpolatedSql value)
        {
            value = (value as ISqlEnricher)?.GetEnrichedSql() ?? value;
            if (AutoSpacing && value.Format.Length > 0)
                CheckAutoSpacing(value.Format[0]);
            Insert(_format.Length, value);
            ResetAutoSpacing();
        }


        /// <summary>
        /// Appends to this instance another InterpolatedString, depending on a condition.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString will be parsed/evaluated but won't be appended.
        /// </summary>
        public void AppendIf(bool condition, IInterpolatedSql value)
        {
            if (!condition)
                return;
            value = (value as ISqlEnricher)?.GetEnrichedSql() ?? value;
            if (AutoSpacing && value.Format.Length > 0)
                CheckAutoSpacing(value.Format[0]);
            Insert(_format.Length, value);
            ResetAutoSpacing();
        }

        /// <summary>
        /// Exactly like <see cref="Append(IInterpolatedSql)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public void AppendLine(IInterpolatedSql value)
        {
            AppendLine();
            value = (value as ISqlEnricher)?.GetEnrichedSql() ?? value;
            Insert(_format.Length, value);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Appends to this instance another interpolated string.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// </summary>
        public void Append([InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            // InterpolatedSqlHandler will get this InterpolatedStringBuilder instance
            // and will receive the literals/arguments to be appended to this instance.

            if (Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            ResetAutoSpacing(); // InterpolatedSqlHandler will set this to false after first write, so we need to restore
        }

        /// <summary>
        /// Appends to this instance another interpolated string, depending on a condition.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// If condition is false, interpolated string won't be parsed or appended.
        /// </summary>
        public void AppendIf(bool condition, [InterpolatedStringHandlerArgument(new[] { "", "condition" })] ref InterpolatedSqlHandler value)
        {
            // InterpolatedSqlHandler will get this InterpolatedStringBuilder instance, and will also get the bool condition.
            // If condition is false, InterpolatedSqlHandler will just early-abort.
            // Else, it will receive the literals/arguments and will append them to this instance.
            if (condition && Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            ResetAutoSpacing(); // InterpolatedSqlHandler will set this to false after first write, so we need to restore
        }

        /// <summary>
        /// Exactly like <see cref="Append(ref InterpolatedSqlHandler)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public void AppendLine(ref InterpolatedSqlHandler value)
        {
            if (Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            AppendLine();
            Append(value.InterpolatedSqlBuilder.AsSql());
        }
#endif
        /// <summary>
        /// Appends to this instance another InterpolatedString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public void Append(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            if (AutoSpacing && value.Format.Length > 0)
                CheckAutoSpacing(value.Format[0]);
            Insert(_format.Length, value);
            ResetAutoSpacing();
        }

        /// <summary>
        /// Appends to this instance another FormattableString, depending on a condition.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString won't be parsed or appended.
        /// </summary>
        public void AppendIf(bool condition, FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            if (!condition)
                return;
            if (AutoSpacing && value.Format.Length > 0)
                CheckAutoSpacing(value.Format[0]);
            Options.Parser.ParseAppend(this, value);
            ResetAutoSpacing();
        }

        /// <inheritdoc/>
        /// <summary>
        /// Exactly like <see cref="Append(FormattableString)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public void AppendLine(FormattableString value
#if NET6_0_OR_GREATER
            , object? dummy = null // to differentiate from InterpolatedSqlHandler overload
#endif
            )
        {
            AppendLine();
            Insert(_format.Length, value);
        }


        /// <summary>
        /// Adds a linebreak
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendLine()
        {
            AppendLiteral(NewLine);
        }

        /// <summary>
        /// Appends to this instance a literal string. 
        /// If you use string interpolation as argument you should NOT use unsafe arguments - this method will NOT parametrize interpolated arguments - they are sent as literals
        /// </summary>
        public void AppendLiteral(string value)
        {
            AppendLiteral(value, 0, value.Length);
            ResetAutoSpacing();
        }

        /// <summary>
        /// Appends to this instance a substring as literal string.
        /// </summary>
        public void AppendLiteral(string value, int startIndex, int count)
        {
            if (count == 0)
                return;

            // If we get a string like "   column = '{variable}'   ", we assume that single quotes are there by mistake (as if variable was a literal string, instead of SqlParameter)
            // so we strip those bad quotes (opening quote was already added in the last AppendLiteral(), and closing quote is being added now
            if (Options.AutoFixSingleQuotes

                // only one argument was written between last literal and this one
                && _lastLiteralArgumentsCount + 1 == _sqlParameters.Count

                // Last literal ended with a single quote
                && _lastLiteralEnd >= 0 && _format[_lastLiteralEnd] == '\'' && (_lastLiteralEnd == 0 || _format[_lastLiteralEnd - 1] != '\'')

                // next literal starts with a single quote
                /*&& count > 0*/ && value[startIndex] == '\'' && (count == 1 || value[startIndex + 1] != '\'')
                )
            {
                _format.Remove(_lastLiteralEnd, 1);
                startIndex++;
                count--;
            }


            if (AutoSpacing && count > 0)
                CheckAutoSpacing(value[startIndex]);

            if (Options.AutoEscapeCurlyBraces == false || value.IndexOfAny(new char[] { '{', '}' }, startIndex, count) == -1)
                _format.Append(value, startIndex, count);
            else
                for (int i = 0; i < count; i++)
                    switch (value[startIndex + i])
                    {
                        case '{':
                            _format.Append("{{");
                            break;
                        case '}':
                            _format.Append("}}");
                            break;
                        default:
                            _format.Append(value[startIndex + i]); break;
                    }

            if (Options.AutoFixSingleQuotes)
            {
                _lastLiteralEnd = _format.Length - 1;
                _lastLiteralArgumentsCount = _sqlParameters.Count;
            }
            ResetAutoSpacing();
        }

        /// <summary>
        /// Appends to this instance another FormattableString.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public void AppendFormattableString(FormattableString value)
        {
            if (AutoSpacing && value.Format.Length > 0)
                CheckAutoSpacing(value.Format[0]);
            Options.Parser.ParseAppend(this, value);
            ResetAutoSpacing();
        }

        /// <summary>
        /// Appends a raw string. 
        /// This is like <see cref="AppendLiteral(string)"/> but it's a little faster since it does not escape curly-braces 
        /// </summary>
        public void AppendRaw(string value)
        {
            _format.Append(value);
        }


        /// <summary>
        /// Given the argument and it's index position (int), this defines how the auto generated parameter name should be named (e.g. add "p" before position).
        /// It's possible to customize this either by overriding method, or by modifying the <see cref="InterpolatedSqlBuilderOptions.CalculateAutoParameterName"/>
        /// </summary>
        protected virtual string CalculateAutoParameterName(InterpolatedSqlParameter argument, int position)
        {
            return Options.CalculateAutoParameterName(argument, position);
        }

        /// <summary>
        /// We assume that a single word will always be appended in a single statement (why would anyone split a single sql word in 2 appends, right?).
        /// This method automatically adds a space if there's no whitespace (or linebreak) between last text and the new text.
        /// </summary>
        protected void CheckAutoSpacing(char? nextChar)
        {
            // so if there is no whitespace (or line break) between the last text and the new text, we add a space betwen them
            // unless it's a known SQL delimiter
            char lastChar;

            // if it's first char of sql
            if (_format.Length == 0)
                return;

            // or if last char was whitespace or any known SQL delimiter or symbol
            lastChar = _format[_format.Length - 1];
            if (char.IsWhiteSpace(lastChar) || Options.AutoSpacingOptions.SeparatorSymbols.Contains(lastChar) || Options.AutoSpacingOptions.OpeningSymbols.Contains(lastChar))
                return;


            // or if next char is whitespace or any known SQL delimiter or symbol
            if (nextChar != null && (char.IsWhiteSpace(nextChar.Value) || Options.AutoSpacingOptions.SeparatorSymbols.Contains(nextChar.Value) || Options.AutoSpacingOptions.ClosingSymbols.Contains(nextChar.Value)))
                return;

            // anything else (including nextChar == null, which means it's a parameter - which will probably be written a "@p0")
            AppendRaw(" ");
        }

        /// <summary>
        /// Returns the index of the start of the contents in a StringBuilder
        /// </summary>        
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        public int IndexOf(string value, int startIndex, bool ignoreCase)
        {
            // Copied from https://stackoverflow.com/questions/1359948/why-doesnt-stringbuilder-have-indexof-method  - it's a shame that StringBuilder has Replace but does not have IndexOf
            int index;
            int length = value.Length;
            int maxSearchLength = _format.Length - length + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                    if (char.ToLower(_format[i]) == char.ToLower(value[0]))
                    {
                        index = 1;
                        while (index < length && char.ToLower(_format[i + index]) == char.ToLower(value[index]))
                            ++index;

                        if (index == length)
                            return i;
                    }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
                if (_format[i] == value[0])
                {
                    index = 1;
                    while (index < length && _format[i + index] == value[index])
                        ++index;

                    if (index == length)
                        return i;
                }

            return -1;
        }

        /// <inheritdoc cref="IndexOf(string, int, bool)"/>
        public int IndexOf(string value, bool ignoreCase = false)
        {
            return IndexOf(value, 0, ignoreCase);
        }

        /// <summary>
        /// Inserts another InterpolatedString at a specific position. Similar to <see cref="Append(IInterpolatedSql)"/>
        /// </summary>
        public void Insert(int index, IInterpolatedSql value)
        {
            value = (value as ISqlEnricher)?.GetEnrichedSql() ?? value;
            if (!value.SqlParameters.Any())
            {
                _format.Insert(index, value.Format);
                return;
            }

            //TODO: maybe we should just modify ParseAppend to start at a given startIndex?

            string newFormat = value.Format;
            if (Options.ReuseIdenticalParameters)
            {
                var oldToNewPos = new Dictionary<int, int>();
                for (int i = 0; i < value.SqlParameters.Count; i++)
                {
                    var parm = value.SqlParameters[i];
                    int newParmPos = AddArgument(parm);
                    oldToNewPos.Add(i, newParmPos);
                }
                Func<int, int> getNewPos = (oldPos) => oldToNewPos.ContainsKey(oldPos) ? oldToNewPos[oldPos] : oldPos;
                newFormat = Options.Parser.ShiftPlaceholderPositions(newFormat, getNewPos);
            }
            else
            {
                int shift = _sqlParameters.Count; // previous arguments
                _sqlParameters.AddRange(value.SqlParameters);
                if (shift > 0) // do new arguments have to be shifted
                {
                    Func<int, int> getNewPos = (oldPos) => oldPos + (oldPos >= 0 && oldPos < value.SqlParameters.Count ? shift : 0);
                    newFormat = Options.Parser.ShiftPlaceholderPositions(newFormat, getNewPos);
                }
            }

            // if (index < _format.Length-1) then the placeholder positions might become out of order - but that's fine
            _format.Insert(index, newFormat);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Inserts another FormattableString at a specific position. Similar to <see cref="AppendFormattableString(FormattableString)"/>
        /// </summary>
        public void Insert(int index, FormattableString value) // TODO: #if NET6_0_OR_GREATER / ref InterpolatedSqlHandler
        {
            //TODO: we could probably use Options.Parser.ParseInsert(value, this, index) - but it's not using ReuseIdenticalParameters, so parsing into InterpolatedSqlBuilder
            var temp = SqlBuilderFactory.Default.Create();
            Options.Parser.ParseAppend(temp, value);
            Insert(index, temp.AsSql());
        }

        /// <summary>
        /// Inserts another literal at a specific position.
        /// </summary>
        public void InsertLiteral(int index, string value)
        {
            _format.Insert(index, value);
        }

        /// <summary>
        /// Removes the specified range of characters from this instance.
        /// </summary>
        public void Remove(int startIndex, int length)
        {
            _format.Remove(startIndex, length);
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by another InterpolatedString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        /// <inheritdoc/>
        public void Replace(string oldValue, IInterpolatedSql newValue)
        {
            Replace(oldValue, newValue, out var _);
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by another InterpolatedString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        public void Replace(string oldValue, IInterpolatedSql newValue, out bool replaced)
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                replaced = false;
            else
            {
                _format.Remove(index, oldValue.Length);
                Insert(index, newValue);
                replaced = true;
            }
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by a FormattableString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        public void Replace(string oldValue, FormattableString newValue) // TODO: #if NET6_0_OR_GREATER / ref InterpolatedSqlHandler
        {
            Replace(oldValue, newValue, out var _);
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by a FormattableString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        public void Replace(string oldValue, FormattableString newValue, out bool replaced) // TODO: #if NET6_0_OR_GREATER / ref InterpolatedSqlHandler
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                replaced = false;
            else
            {
                _format.Remove(index, oldValue.Length);
                Insert(index, newValue);
                replaced = true;
            }
        }

        /// <summary>
        /// Removes all trailing white-space characters
        /// </summary>
        public void TrimEnd() // TODO: params char[] trimChars?
        {
            if (_format == null || _format.Length == 0)
                return;

            int i = _format.Length - 1;

            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(_format[i]))
                    break;

            if (i < _format.Length - 1)
                _format.Length = i + 1;
        }


        #endregion

        #region Explicit Adding Parameters (usually this is only for Stored Procedures)
        /// <summary>
        /// Explicitly adds a single parameter to current Command Builder. <br />
        /// This is usually only for stored procedures where the @parameterName is already defined in the stored procedure.
        /// For building dynamic SQL statements you should just append interpolated strings and interpolated the parameters:
        /// it's possible to interpolate any primitive type (string, int, double, etc - Dapper/ORM will convert to SqlParameter),
        /// or you can interpolate <see cref="SqlParameterInfo"/> or even System.Data.SqlParameter.
        /// </summary>
        public virtual void AddParameter(string parameterName, object? parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            AddParameter(new DbTypeParameterInfo(parameterName, parameterValue, direction, dbType, size, precision, scale));
        }

        /// <summary>
        /// In most cases SqlParameters can be passed as arguments interpolated within an interpolated string.
        /// 
        /// This is an alternative method to explicitly add parameters to the CommandBuilder (usually for Stored Procedures)
        /// These parameters are parametrized like all other parameters, but they will not generate placeholders
        /// (initially numbered like "{0}" and later converted to names like "@p0").
        /// </summary>
        public void AddParameter(SqlParameterInfo parameterInfo)
        {
            _explicitParameters.Add(parameterInfo);
        }

        /// <summary>
        /// Adds all public properties of an object (like a POCO) as parameters of the current Command Builder. <br />
        /// This is like Dapper templates: useful when you're passing an object with multiple properties and you'll reference those properties in the SQL statement. <br />
        /// This method does not check for name clashes against previously added parameters. <br />
        /// </summary>
        public virtual void AddObjectProperties(object obj)
        {
            var props =
                obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop);

            foreach (var prop in props)
                AddParameter(new DbTypeParameterInfo(prop.Key, prop.Value.GetValue(obj, new object[] { })));
        }
        #endregion

        #region Conversions and Debugging/Display
        /// <summary>
        /// Returns the generated SQL statement followed by the value of the parameters (surrounded by parentheses).
        /// This is NOT supposed to be executed as a command (it shouldn't even be a valid statement).
        /// The values that can be executed against a database are <see cref="Sql"/> and <see cref="SqlParameters"/>,
        /// </summary>
        public override string ToString()
        {
            return DebuggerDisplay;
        }

        private string DebuggerDisplay
        {
            get
            {
                if (_format == null || _sqlParameters == null || _explicitParameters == null)
                    return ""; // not initialized
                var built = (this as ISqlEnricher)?.GetEnrichedSql() ?? this.AsSql();
                if (built == null)
                    return "";
                var parms = built.SqlParameters;
                if (parms == null)
                    return "";
                if (!parms.Any())
                    return "\"" + built.Sql + "\"";
                var formattedParms = parms.Select(
                    (parm, pos) => Options.FormatParameterName(CalculateAutoParameterName(parm, pos))
                    + "="
                    + (parms[pos].Argument is string ? "'" : "")
                    + (parms[pos].Argument?.ToString() ?? "")
                    + (parms[pos].Argument is string ? "'" : "")
                    );
                return "\"" + built.Sql + "\"" + " (" + string.Join(", ", formattedParms) + ")";
            }
        }

        /// <summary>
        /// Casts the current builder into an IInterpolatedSql using a wrapper.
        /// The underlying properties (Sql, SqlParameters, etc) are the same - they are not copied.
        /// If the current builder implements <see cref="ISqlEnricher"/>  this will return that enriched result.
        /// </summary>
        public IInterpolatedSql AsSql()
        {
            if (this is ISqlEnricher enricher)
                return enricher.GetEnrichedSql();
            return new SqlBuilderWrapper(this);
        }

        /// <summary>
        /// Return an immutable <see cref="IInterpolatedSql"/>.
        /// Similar to <see cref="AsSql"/> but creates a copy of the values instead of a wrapper
        /// </summary>
        public IInterpolatedSql ToSql()
        {
            if (this is ISqlEnricher enricher)
            {
                var sql = enricher.GetEnrichedSql();
                if (ReferenceEquals(sql, this)) // force a copy
                    return new ImmutableInterpolatedSql(sql.Sql, sql.Format, sql.SqlParameters, sql.ExplicitParameters);
                return sql;
            }
            string format = _format.ToString();
            return new ImmutableInterpolatedSql(BuildSql(format, _sqlParameters), format, _sqlParameters, _explicitParameters);
        }

        /// <summary>
        /// Lightweight wrapper that will expose the properties of an underlying InterpolatedSqlBuilder
        /// </summary>
        protected class SqlBuilderWrapper : IInterpolatedSql
        {
            private readonly InterpolatedSqlBuilderBase _underlyingBuilder;

            /// <inheritdoc />
            public SqlBuilderWrapper(InterpolatedSqlBuilderBase underlyingBuilder)
            {
                _underlyingBuilder = underlyingBuilder;
            }

            /// <inheritdoc cref="IInterpolatedSql.Format" />
            public string Format => _underlyingBuilder._format.ToString();

            /// <inheritdoc cref="IInterpolatedSql.SqlParameters" />
            public IReadOnlyList<InterpolatedSqlParameter> SqlParameters => _underlyingBuilder._sqlParameters;

            /// <inheritdoc cref="IInterpolatedSql.ExplicitParameters" />
            public IReadOnlyList<SqlParameterInfo> ExplicitParameters => _underlyingBuilder._explicitParameters;

            /// <inheritdoc cref="IInterpolatedSql.Sql" />
            public string Sql => _underlyingBuilder.BuildSql(_underlyingBuilder._format.ToString(), _underlyingBuilder._sqlParameters);
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator FormattableString(InterpolatedSqlBuilderBase builder)
        {
            return builder.AsFormattableString();
        }

        /// <summary>
        /// Converts to FormattableString (composite format)
        /// </summary>
        /// <returns></returns>
        public FormattableString AsFormattableString()
        {
            var sql = AsSql();
            return FormattableStringFactory.Create(sql.Format, sql.SqlParameters.ToArray());
        }
        #endregion
    }

}
