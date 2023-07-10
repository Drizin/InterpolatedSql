using System;
using System.Diagnostics;

namespace InterpolatedSql
{
    /// <summary>
    /// LegacyFormattableString is just a wrapper around FormattableStrings (with implicit conversion from FormattableString to it) 
    /// which allows the compiler to prioritize methods that use InterpolatedSqlHandler (InterpolatedStringHandler) instead of FormattableStrings<br />
    /// For net6.0+ the compiler should pick InterpolatedSqlHandler overload, 
    /// while for older versions (where InterpolatedSqlHandler overload are not available) it should implicitly convert FormattableString to this LegacyFormattableString wrapper, <br />
    /// so net6.0+ can offer both overloads without causing method call ambiguity.
    /// </summary>
    [DebuggerDisplay("{_value}")]
    public class LegacyFormattableString
    {
        private FormattableString _value { get; }

        private LegacyFormattableString(FormattableString value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public static implicit operator LegacyFormattableString(FormattableString value) => new LegacyFormattableString(value);

        /// <inheritdoc/>
        public static implicit operator FormattableString(LegacyFormattableString value) => value._value;
    }
}
