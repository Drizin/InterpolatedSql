#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace InterpolatedSql
{
    /// <summary>
    /// Just a simple InterpolatedStringHandler that builds a <see cref="InterpolatedSqlBuilder"/> without requiring regex parsing.
    /// </summary>
    [InterpolatedStringHandler]
    [DebuggerDisplay("{InterpolatedSqlBuilder}")]
    public ref struct InterpolatedSqlHandler
    {
        /// <summary>
        /// Underlying Interpolated String
        /// </summary>
        public InterpolatedSqlBuilderBase InterpolatedSqlBuilder => _interpolatedSqlBuilder;

        private InterpolatedSqlBuilderBase _interpolatedSqlBuilder;

        private bool _checkAutospace;

        /// <summary>
        /// Position of the underlying InterpolatedSqlBuilder when this InterpolatedSqlHandler started writing into it.
        /// </summary>
        private int _sqlBuilderStartingPos;



        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount) // InterpolatedStringFactory.Create() doesn't provide "this" (InterpolatedSqlHandler will create a new StringBuilder)
        {
            _interpolatedSqlBuilder = new InterpolatedSqlBuilder(literalLength, formattedCount, null);
            _sqlBuilderStartingPos = 0;
            _checkAutospace = _interpolatedSqlBuilder.Options.AutoSpace;
        }

        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount, InterpolatedSqlBuilderBase target) // used by InterpolatedSqlBuilder.Append(...)
        {
            _interpolatedSqlBuilder = target;
            _sqlBuilderStartingPos = target.Length;
            _checkAutospace = _interpolatedSqlBuilder.Options.AutoSpace;
        }

        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount, InterpolatedSqlBuilder target, bool condition, out bool isEnabled) // used by InterpolatedSqlBuilder.AppendIf(bool condition, ...)
        {
            isEnabled = condition;
            if (!isEnabled)
            {
                _interpolatedSqlBuilder = null;
                _sqlBuilderStartingPos = -1;
                _checkAutospace = false;
                return;
            }
            _interpolatedSqlBuilder = target;
            _sqlBuilderStartingPos = target.Length;
            _checkAutospace = _interpolatedSqlBuilder.Options.AutoSpace;
        }

        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions options)
        {
            _interpolatedSqlBuilder = new InterpolatedSqlBuilder(literalLength, formattedCount, options);
            _sqlBuilderStartingPos = 0;
            _checkAutospace = _interpolatedSqlBuilder.Options.AutoSpace;
        }

        /// <summary>
        /// Appends a literal string. 
        /// </summary>
        public void AppendLiteral(string value)
        {
            if (_checkAutospace && value.Length > 1)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(value[0]);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendLiteral(value);
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendArgument(value.ToString());
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(string? value)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendArgument(value?.ToString());
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(string? value, int alignment = 0, string? format = null)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendArgument(value, alignment, format);
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(object? value, int alignment = 0, string? format = null)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendArgument(value, alignment, format);
        }


        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted<T>(T value)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }

            // If we get a nested IInterpolatedSql (including InterpolatedSqlBuilder or any derived type), it's already parsed - we just merge to current one
            if (value is IInterpolatedSql isbArg)
            {
                _interpolatedSqlBuilder.Append(isbArg); // this will automatically shift the arguments
                return;
            }

            // If we get a nested FormattableString, we parse it (recursively) and merge it to current one
            if (value is FormattableString fsArg)
            {
                _interpolatedSqlBuilder.Append(fsArg); // this will automatically shift the arguments
                return;
            }

            _interpolatedSqlBuilder.AppendArgument(value, 0, null);
        }

        /// <summary>
        /// Appends the specified object.
        /// </summary>
        public void AppendFormatted<T>(T value, string format)
        {
            if (_checkAutospace)
            {
                _interpolatedSqlBuilder.CheckAutoSpace(null);
                _checkAutospace = false;
            }
            _interpolatedSqlBuilder.AppendArgument(value, 0, format);
        }


        
        /// <summary>
        /// Removes the left padding of the whole string that was just processed.
        /// See InterpolatedSqlParser.AdjustMultilineString() for more info
        /// </summary>
        public void AdjustMultilineString()
        {
            //TODO: process the first few characters of _interpolatedSqlBuilder.Format[_sqlBuilderStartingPos...], ignore first linebreak (if exists), and check if it starts with whitespace.
            // if it's not whitespace then we don't even need to extract the string and try the expensive reformatting.
            string format = _interpolatedSqlBuilder.Format.Substring(_sqlBuilderStartingPos, _interpolatedSqlBuilder.Length - _sqlBuilderStartingPos);
            string adjustedFormat = _interpolatedSqlBuilder.Options.Parser.AdjustMultilineString(format);
            if (!format.Equals(adjustedFormat))
            {
                _interpolatedSqlBuilder.Remove(_sqlBuilderStartingPos, _interpolatedSqlBuilder.Length-_sqlBuilderStartingPos);
                _interpolatedSqlBuilder.InsertLiteral(_sqlBuilderStartingPos, adjustedFormat);
            }                
        }

    }
}
#endif

