#if NET6_0_OR_GREATER
using InterpolatedSql.SqlBuilders;
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
    public ref struct InterpolatedSqlHandler<B>
        where B : IInterpolatedSqlBuilderBase, new()
    {
        /// <summary>
        /// Underlying Interpolated String
        /// </summary>
        public B InterpolatedSqlBuilder => _interpolatedSqlBuilder;

        private readonly B _interpolatedSqlBuilder;

        /// <summary>
        /// Position of the underlying InterpolatedSqlBuilder when this InterpolatedSqlHandler started writing into it.
        /// </summary>
        private int _sqlBuilderStartingPos;

        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount) // InterpolatedSqlFactory.Create() doesn't provide "this" (InterpolatedSqlHandler will create a new StringBuilder)
        {
            _interpolatedSqlBuilder = InterpolatedSqlBuilderFactory<B>.Default.Create(literalLength, formattedCount);
            _sqlBuilderStartingPos = 0;
        }

        /// <inheritdoc />
        public InterpolatedSqlHandler(int literalLength, int formattedCount, InterpolatedSqlBuilderOptions options)
        {
            _interpolatedSqlBuilder = InterpolatedSqlBuilderFactory<B>.Default.Create(literalLength, formattedCount, options);
            _sqlBuilderStartingPos = 0;
        }

        /// <summary>
        /// Appends a literal string. 
        /// </summary>
        public void AppendLiteral(string value)
        {
            _interpolatedSqlBuilder.AppendLiteral(value);
            _interpolatedSqlBuilder.AutoSpacing = false; // Autospacing should be applied only to the whole interpolated block, not to each individual literal/argument. //TODO: use SCOPES in SqlBuilder and scoped-variables
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            _interpolatedSqlBuilder.AppendArgument(value.ToString());
            _interpolatedSqlBuilder.AutoSpacing = false;
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(string? value)
        {
            _interpolatedSqlBuilder.AppendArgument(value?.ToString());
            _interpolatedSqlBuilder.AutoSpacing = false;
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(string? value, int alignment = 0, string? format = null)
        {
            _interpolatedSqlBuilder.AppendArgument(value, alignment, format);
            _interpolatedSqlBuilder.AutoSpacing = false;
        }

        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted(object? value, int alignment = 0, string? format = null)
        {
            _interpolatedSqlBuilder.AppendArgument(value, alignment, format);
            _interpolatedSqlBuilder.AutoSpacing = false;
        }


        /// <summary>Appends the specified object.</summary>
        public void AppendFormatted<T>(T value)
        {
            // If we get a nested IInterpolatedSql (including InterpolatedSqlBuilder or any derived type), it's already parsed - we just merge to current one
            if (value is IInterpolatedSql isqlArg)
            {
                _interpolatedSqlBuilder.Append(isqlArg); // this will automatically shift the arguments
                _interpolatedSqlBuilder.AutoSpacing = false;
                return;
            }

            if (value is ISqlEnricher enricher)
            {
                _interpolatedSqlBuilder.Append(enricher.GetEnrichedSql()); // this will automatically shift the arguments
                _interpolatedSqlBuilder.AutoSpacing = false;
                return;
            }

            // If we get a nested FormattableString, we parse it (recursively) and merge it to current one
            if (value is FormattableString fsArg)
            {
                _interpolatedSqlBuilder.AppendFormattableString(fsArg); // this will automatically shift the arguments
                _interpolatedSqlBuilder.AutoSpacing = false;
                return;
            }

            if (value is InterpolatedSqlBuilderBase builder)
            {
                _interpolatedSqlBuilder.Append(builder.AsSql()); // this will automatically shift the arguments
                _interpolatedSqlBuilder.AutoSpacing = false;
                return;
            }

            _interpolatedSqlBuilder.AppendArgument(value, 0, null);
            _interpolatedSqlBuilder.AutoSpacing = false;
        }

        /// <summary>
        /// Appends the specified object.
        /// </summary>
        public void AppendFormatted<T>(T value, string format)
        {
            _interpolatedSqlBuilder.AppendArgument(value, 0, format);
            _interpolatedSqlBuilder.AutoSpacing = false;
        }

        
        /// <summary>
        /// Removes the left padding of the whole string that was just processed.
        /// See InterpolatedSqlParser.AdjustMultilineString() for more info
        /// </summary>
        public void AdjustMultilineString()
        {
            //TODO: process the first few characters of _interpolatedSqlBuilder.Format[_sqlBuilderStartingPos...], ignore first linebreak (if exists), and check if it starts with whitespace.
            // if it's not whitespace then we don't even need to extract the string and try the expensive reformatting.
            string format = _interpolatedSqlBuilder.Format.Substring(_sqlBuilderStartingPos, _interpolatedSqlBuilder.Format.Length - _sqlBuilderStartingPos);
            string adjustedFormat = _interpolatedSqlBuilder.Options.Parser.AdjustMultilineString(format);
            if (!format.Equals(adjustedFormat))
            {
                _interpolatedSqlBuilder.Remove(_sqlBuilderStartingPos, _interpolatedSqlBuilder.Format.Length - _sqlBuilderStartingPos);
                _interpolatedSqlBuilder.InsertLiteral(_sqlBuilderStartingPos, adjustedFormat);
            }
        }

    }
}
#endif
