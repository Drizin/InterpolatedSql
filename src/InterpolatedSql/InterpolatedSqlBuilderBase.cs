using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// Dynamic SQL builder where SqlParameters are defined using string interpolation (but it's injection safe).
    /// Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
    /// and will be parametrized when you need to run the command.
    /// So it wraps the underlying SQL statement and the associated parameters, 
    /// allowing to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class InterpolatedSqlBuilderBase : IInterpolatedSql
    {
        #region Constants/Static
        /// <summary>
        /// Default options used when options is not defined in constructor
        /// </summary>
        public static InterpolatedSqlBuilderOptions DefaultOptions { get; set; } = new InterpolatedSqlBuilderOptions();
        #endregion

        #region Members
        /// <inheritdoc cref="InterpolatedSqlBuilderOptions"/>
        public InterpolatedSqlBuilderOptions Options { get; set; }

        /// <summary>
        /// This is like <see cref="FormattableString.Format"/> which is the underlying format of the interpolated string.
        /// </summary>
        public virtual string Format
        {
            get
            {
                if (_cachedFormat == null)
                    _cachedFormat = _format.ToString();
                return _cachedFormat;
            }
        }

        /// <inheritdoc cref="Format" />
        protected StringBuilder _format;

        /// <summary>
        /// <see cref="Format"/> uses <see cref="StringBuilder.ToString()"/> which is slow - this is a cache for faster reuse. Cache can be invalidated by setting it to null, so it gets recalculated when needed
        /// </summary>
        protected string? _cachedFormat = null;

        /// <summary>
        /// Sql Parameters that were embedded using string interpolation (actually they can be any object type) 
        /// </summary>
        protected List<InterpolatedSqlParameter> _sqlParameters;

        /// <summary>
        /// Sql Parameters that were embedded using string interpolation (actually they can be any object type) 
        /// </summary>
        public virtual IReadOnlyList<InterpolatedSqlParameter> SqlParameters => _sqlParameters;

        /// <summary>
        /// Explicit parameters (named, and added explicitly - not using interpolated arguments)
        /// </summary>
        protected List<SqlParameterInfo> _explicitParameters = new List<SqlParameterInfo>();

        /// <summary>
        /// Explicit Parameters
        /// </summary>
        public IReadOnlyList<SqlParameterInfo> ExplicitParameters => _explicitParameters;


        /// <summary>
        /// Optional associated DbConnection. Can be used in custom extensions.
        /// (but instead of using this nullable property please consider creating a subtype and hiding it with a non-nullable property - see InterpolatedSql.Dapper.SqlBuilder)
        /// </summary>
        public IDbConnection? DbConnection { get; set; }

        /// <summary>
        /// Object bag - can be used in custom extensions (but consider creating a subtype instead of using this)
        /// </summary>
        public Dictionary<string, object>? ObjectBag { get; set; }

        /// <summary>
        /// By default this is <see cref="Environment.NewLine"/>
        /// </summary>
        protected virtual string NewLine { get; set; } = Environment.NewLine;


        private int _lastLiteralEnd = -1;             // position where last literal char was written
        private int _lastLiteralArgumentsCount = 0;   // how many arguments were there when the last literal was added

        /// <summary>
        /// SQL of Command
        /// </summary>
        public virtual string Sql
        {
            get
            {
                if (_cachedSql == null)
                {
                    if (SqlParameters.Any())
                        _cachedSql = string.Format(Format, SqlParameters.Select((parm, pos) => Options.FormatParameterName(CalculateAutoParameterName(parm, pos))).ToArray());
                    else
                        _cachedSql = Format;
                }
                    
                return _cachedSql ?? "";
            }
        }

        /// <inheritdoc cref="Sql" />
        protected string? _cachedSql = null;

        /// <summary>
        /// Checks if there were no literals and no SqlParameters added
        /// </summary>
        public bool IsEmpty => Format.Length == 0 && !SqlParameters.Any();

        /// <summary>
        /// Length of the underlying format (which includes literals and argument placeholders)
        /// </summary>
        public int Length => Format.Length;


        /// <summary>
        /// Preview of parameters and values - just for debugging
        /// </summary>
        protected string SqlParametersDebugPreview
        {
            get
            {
                if (_cachedSqlParameters == null && SqlParameters.Any())
                {
                    var parms = SqlParameters.Select(
                        (parm, pos) => Options.FormatParameterName(CalculateAutoParameterName(parm, pos)) 
                        + "=" 
                        + (SqlParameters[pos].Argument is string ? "'" : "")
                        + (SqlParameters[pos].Argument?.ToString() ?? "")
                        + (SqlParameters[pos].Argument is string ? "'" : "")
                        );
                    _cachedSqlParameters = string.Join(", ", parms);
                }
                return _cachedSqlParameters ?? "";
            }
        }

        /// <inheritdoc cref="SqlParametersDebugPreview" />
        protected string? _cachedSqlParameters = null;



        #endregion

        #region ctor
        /// <inheritdoc cref="InterpolatedSqlBuilderBase" />
        protected InterpolatedSqlBuilderBase(InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments)
        {
            Options = options ?? DefaultOptions;
            _format = format ?? new StringBuilder();
            _sqlParameters = arguments ?? new List<InterpolatedSqlParameter>();
        }

        /// <inheritdoc cref="InterpolatedSqlBuilderBase" />
        public InterpolatedSqlBuilderBase(InterpolatedSqlBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
            // Since this constructor doesn't parse anything immediately,
            // options could be defined after constructor (e.g. in initializer)
            // - shouldn't make a difference, as long as options are configured before parsing the first string
        }


        /// <inheritdoc cref="InterpolatedSqlBuilderBase" />
        public InterpolatedSqlBuilderBase(FormattableString value, InterpolatedSqlBuilderOptions? options = null) : this(options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be important to provide Options (and Parser) immediately together
            if (value != null)
                Options.Parser.ParseAppend(value, this);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc cref="InterpolatedSqlBuilderBase" />
        public InterpolatedSqlBuilderBase(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
        {
            Options = options ?? DefaultOptions;
            _format = new StringBuilder(literalLength + formattedCount*4); // embedded parameter will usually occupy 3-4 chars ("{0}"..."{99}")
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
            {
                // Reuse existing parameters (don't pass duplicates)
                for (int i = 0; i < _sqlParameters.Count; i++)
                    if (Options.ArgumentComparer.Equals(_sqlParameters[i], argument))
                        return i;
            }

            _sqlParameters.Add(argument);
            return _sqlParameters.Count - 1;
        }

        /// <summary>
        /// Appends to this instance another InterpolatedString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public virtual void Append(LegacyFormattableString value)
        {
            if (Options.AutoSpace && ((FormattableString)value).Format.Length > 1)
                CheckAutoSpace(((FormattableString)value).Format[0]);
            Insert(_format.Length, (FormattableString)value);
        }

        /// <summary>
        /// Exactly like <see cref="Append(LegacyFormattableString)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public virtual void AppendLine(LegacyFormattableString value)
        {
            AppendLiteral(NewLine);
            Insert(_format.Length, value);
        }

        /// <summary>
        /// Adds a linebreak
        /// </summary>
        public virtual void AppendLine()
        {
            AppendLiteral(NewLine);
        }


        /// <summary>
        /// Appends to this instance another InterpolatedString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public virtual void Append(IInterpolatedSql value)
        {
            if (Options.AutoSpace && value.Format.Length > 1)
                CheckAutoSpace(value.Format[0]);
            Insert(_format.Length, value);
        }

        /// <summary>
        /// Exactly like <see cref="Append(IInterpolatedSql)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public virtual void AppendLine(IInterpolatedSql value)
        {
            AppendLiteral(NewLine);
            Insert(_format.Length, value);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Appends to this instance another interpolated string.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// </summary>
        public virtual void Append([InterpolatedStringHandlerArgument("")] ref InterpolatedSqlHandler value)
        {
            // InterpolatedSqlHandler will get this InterpolatedStringBuilder instance
            // and will receive the literals/arguments to be appended to this instance.

            if (Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
        }

        /// <summary>
        /// Exactly like <see cref="Append(ref InterpolatedSqlHandler)"/>, but adds a linebreak BEFORE the provided value.
        /// The idea is that in a single statement we can add a new line and isolate it from the PREVIOUS line
        /// (there's no point in having linebreaks at the END of a query)
        /// </summary>
        public virtual void AppendLine(ref InterpolatedSqlHandler value)
        {
            if (Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            AppendLiteral(NewLine);
            Append(value.InterpolatedSqlBuilder);
        }
#endif

        /// <summary>
        /// Appends to this instance another FormattableString.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public virtual void AppendFormattableString(FormattableString value)
        {
            if (Options.AutoSpace && value.Format.Length > 1)
                CheckAutoSpace(value.Format[0]);
            Options.Parser.ParseAppend(value, this);
        }


        /// <summary>
        /// Appends to this instance another InterpolatedString, depending on a condition.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString will be parsed/evaluated but won't be appended.
        /// </summary>
        public virtual void AppendIf(bool condition, IInterpolatedSql value)
        {
            if (!condition)
                return;
            if (Options.AutoSpace && value.Format.Length > 1)
                CheckAutoSpace(value.Format[0]);
            Insert(_format.Length, value);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Appends to this instance another interpolated string, depending on a condition.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// If condition is false, interpolated string won't be parsed or appended.
        /// </summary>
        public virtual void AppendIf(bool condition, [InterpolatedStringHandlerArgument("", "condition")] ref InterpolatedSqlHandler value)
        {
            // InterpolatedSqlHandler will get this InterpolatedStringBuilder instance, and will also get the bool condition.
            // If condition is false, InterpolatedSqlHandler will just early-abort.
            // Else, it will receive the literals/arguments and will append them to this instance.
            if (condition && Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
        }
#endif

        /// <summary>
        /// Appends to this instance another FormattableString, depending on a condition.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString won't be parsed or appended.
        /// </summary>
        public virtual void AppendIf(bool condition, LegacyFormattableString value)
        {
            if (!condition)
                return;
            if (Options.AutoSpace && ((FormattableString)value).Format.Length > 1)
                CheckAutoSpace(((FormattableString)value).Format[0]);
            Options.Parser.ParseAppend((FormattableString)value, this);
        }

        /// <summary>
        /// Appends an argument.
        /// This will both add the argument object to the list of arguments (see <see cref="AddArgument(InterpolatedSqlParameter)"/>)
        /// and will also add the placeholder to the format StringBuilder (e.g. "{2}")
        /// </summary>
        protected internal virtual void AppendArgument(object? argument, int alignment = 0, string? format = null)
        {
            if (format == "raw")
            {
                AppendRaw(argument?.ToString() ?? "");
                return;
            }

            int argumentPos = AddArgument(new InterpolatedSqlParameter(argument, format));
            _format.Append("{");
            _format.Append(argumentPos);
            if (alignment != 0)
                _format.Append("," + alignment.ToString());
            if (Options.PreserveArgumentFormatting && !string.IsNullOrEmpty(format))
                _format.Append(":").Append(format);
            _format.Append("}");
            PurgeLiteralCache();
            PurgeParametersCache();
        }

        /// <summary>
        /// Appends to this instance a literal string. 
        /// If you use string interpolation as argument you should NOT use unsafe arguments - this method will NOT parametrize interpolated arguments - they are sent as literals
        /// </summary>
        public virtual void AppendLiteral(string value)
        {
            AppendLiteral(value, 0, value.Length);
        }

        /// <summary>
        /// Appends to this instance a substring as literal string.
        /// </summary>
        public virtual void AppendLiteral(string value, int startIndex, int count)
        {
            if (count == 0)
                return;

            // If we get a string like "   column = '{variable}'   ", we assume that single quotes are there by mistake (as if variable was a literal string, instead of SqlParameter)
            // so we strip those bad quotes (opening quote was already added in the last AppendLiteral(), and closing quote is being added now
            if (Options.AutoFixSingleQuotes 

                // only one argument was written between last literal and this one
                && _lastLiteralArgumentsCount + 1 == _sqlParameters.Count

                // Last literal ended with a single quote
                && _lastLiteralEnd >= 0 && _format[_lastLiteralEnd] == '\'' && (_lastLiteralEnd == 0 || _format[_lastLiteralEnd-1] != '\'')

                // next literal starts with a single quote
                /*&& count > 0*/ && value[startIndex] == '\'' && (count == 1 || value[startIndex+1] != '\'')
                )
            {
                _format.Remove(_lastLiteralEnd, 1);
                startIndex++;
                count--;
            }


            if (Options.AutoEscapeCurlyBraces == false || value.IndexOfAny(new char[] { '{', '}' }, startIndex, count) == -1)
            {
                _format.Append(value, startIndex, count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
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
                }
            }
            PurgeLiteralCache();

            if (Options.AutoFixSingleQuotes)
            {
                _lastLiteralEnd = _format.Length - 1;
                _lastLiteralArgumentsCount = _sqlParameters.Count;
            }
        }

        /// <summary>
        /// Appends a raw string. 
        /// This is like <see cref="AppendLiteral(string)"/> but it's a little faster since it does not escape curly-braces 
        /// </summary>
        public virtual void AppendRaw(string value)
        {
            _format.Append(value);
            PurgeLiteralCache();
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
        protected internal void CheckAutoSpace(char? nextChar)
        {
            // so if there is no whitespace (or line break) between the last text and the new text, we add a space betwen them
            // unless it's a known SQL delimiter
            char lastChar;
            
            // if it's first char of sql, or if last char was whitespace or any known SQL delimiter
            if (_format.Length == 0
                || char.IsWhiteSpace(lastChar = _format[_format.Length - 1])
                || lastChar == ','
                || lastChar == ';'
                || lastChar == '('
                //|| lastChar == ')'
                )
                return;

            // If next char is whitespace or any known SQL delimiter
            if (nextChar != null && (
                char.IsWhiteSpace(nextChar.Value)
                || nextChar.Value == ','
                || nextChar.Value == ';'
                //|| nextChar.Value == '('
                || nextChar.Value == ')'
                ))
                return;

            // anything else (including nextChar == null, which means it's a parameter - which will probably be written a "@p0")
            AppendRaw(" ");
            PurgeLiteralCache();
        }

        // From https://stackoverflow.com/questions/1359948/why-doesnt-stringbuilder-have-indexof-method  - it's a shame that StringBuilder has Replace but does not have IndexOf
        /// <summary>
        /// Returns the index of the start of the contents in a StringBuilder
        /// </summary>        
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        public int IndexOf(string value, int startIndex, bool ignoreCase)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (_format.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (char.ToLower(_format[i]) == char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (char.ToLower(_format[i + index]) == char.ToLower(value[index])))
                            ++index;

                        if (index == length)
                            return i;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (_format[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (_format[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        /// <inheritdoc cref="IndexOf(string, int, bool)"/>
        public int IndexOf(string value, bool ignoreCase = false)
        {
            return IndexOf(value, 0, ignoreCase);
        }


        /// <summary>
        /// Inserts another FormattableString at a specific position. Similar to <see cref="AppendFormattableString(FormattableString)"/>
        /// </summary>
        public virtual void Insert(int index, LegacyFormattableString value) // TODO: #if NET6_0_OR_GREATER / ref InterpolatedSqlHandler
        {
            //TODO: we could probably use Options.Parser.ParseInsert(value, this, index) - but it's not using ReuseIdenticalParameters, so parsing into InterpolatedSqlBuilder
            var temp = new InterpolatedSqlBuilder();
            Options.Parser.ParseAppend((FormattableString)value, temp);
            Insert(index, temp);
        }

        /// <summary>
        /// Inserts another InterpolatedString at a specific position. Similar to <see cref="Append(IInterpolatedSql)"/>
        /// </summary>
        public virtual void Insert(int index, IInterpolatedSql value)
        {
            if (!value.SqlParameters.Any())
            {
                _format.Insert(index, value.Format);
                PurgeLiteralCache();
                return;
            }


            //TODO: maybe we should just modify ParseAppend to start at a given startIndex?

            string newFormat = value.Format;
            if (Options.ReuseIdenticalParameters)
            {
                Dictionary<int, int> oldToNewPos = new Dictionary<int, int>();
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
                    Func<int, int> getNewPos = (oldPos) => oldPos + ((oldPos >= 0 && oldPos < value.SqlParameters.Count) ? shift : 0);
                    newFormat = Options.Parser.ShiftPlaceholderPositions(newFormat, getNewPos);
                }
            }

            // if (index < _format.Length-1) then the placeholder positions might become out of order - but that's fine
            _format.Insert(index, newFormat);
            PurgeLiteralCache();
            PurgeParametersCache();
        }

        /// <summary>
        /// Inserts another literal at a specific position.
        /// </summary>
        public virtual void InsertLiteral(int index, string value)
        {
            _format.Insert(index, value);
            PurgeLiteralCache();
        }



        /// <summary>
        /// Removes the specified range of characters from this instance.
        /// </summary>
        public virtual void Remove(int startIndex, int length)
        {
            _format.Remove(startIndex, length);
            PurgeLiteralCache();
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by another InterpolatedString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        public virtual bool Replace(string oldValue, LegacyFormattableString newValue) // TODO: #if NET6_0_OR_GREATER / ref InterpolatedSqlHandler
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                return false;

            _format.Remove(index, oldValue.Length);
            Insert(index, newValue);
            return true;
        }

        /// <inheritdoc cref="Replace(string, LegacyFormattableString)"/>
        public virtual bool Replace(string oldValue, IInterpolatedSql newValue)
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                return false;

            _format.Remove(index, oldValue.Length);
            Insert(index, newValue);
            return true;
        }

        /// <summary>
        /// Removes all trailing white-space characters
        /// </summary>
        public virtual void TrimEnd() // TODO: params char[] trimChars?
        {
            if (_format == null || _format.Length == 0) 
                return;

            int i = _format.Length - 1;

            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(_format[i]))
                    break;

            if (i < _format.Length - 1)
                _format.Length = i + 1;
            PurgeLiteralCache();
        }

        /// <summary>
        /// Returns the generated SQL statement followed by the value of the parameters (surrounded by parentheses).
        /// This is NOT supposed to be executed as a command (it shouldn't even be a valid statement).
        /// The values that can be executed against a database are <see cref="Sql"/> and <see cref="SqlParameters"/>,
        /// </summary>
        public override string ToString() => "[DEBUG]: " + DebuggerDisplay;

        private string DebuggerDisplay => "\"" + Sql + "\"" + (string.IsNullOrEmpty(SqlParametersDebugPreview) ? "" : (" (" + SqlParametersDebugPreview + ")"));

        /// <summary>
        /// Purges stringbuilder cached variables (some of which are only used for debugger display)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PurgeLiteralCache()
        {
            _cachedFormat = null;
            _cachedSql = null;
        }

        /// <summary>
        /// Purges Parameters cache (only used for debugger display)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PurgeParametersCache()
        {
            _cachedSqlParameters = null;
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator FormattableString(InterpolatedSqlBuilderBase builder) => FormattableStringFactory.Create(builder.Format, builder.SqlParameters.ToArray());


        #endregion

        #region Explicit Adding Parameters
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
        public virtual void AddParameter(SqlParameterInfo parameterInfo)
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
            Dictionary<string, PropertyInfo> props =
                obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop);

            foreach (var prop in props)
            {
                AddParameter(new DbTypeParameterInfo(prop.Key, prop.Value.GetValue(obj, new object[] { })));
            }
        }
        #endregion

    }

}