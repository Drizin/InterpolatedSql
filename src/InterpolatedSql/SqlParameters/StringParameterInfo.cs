using System.Data;

namespace InterpolatedSql
{
    /// <summary>
    /// Represents a Sql Parameter of String type.
    /// Copied from Dapper DbString.
    /// Most other types can use <see cref="DbTypeParameterInfo"/> (or can just pass the value, without using a SqlParameterInfo subtype),
    /// but strings have some important aspects: 
    /// - Fixed Length or Variable Length (in MSSQL fixed length would be char/nchar, and variable would be varchar/nvarchar)
    /// - Ansi vs Unicode (in MSSQL Ansi would be char/varchar, while Unicode would be nchar/nvarchar).
    /// </summary>
    public class StringParameterInfo : SqlParameterInfo
    {
        #region Members
        /// <summary>
        /// Default value for IsAnsi.
        /// </summary>
        public static bool IsAnsiDefault { get; set; }

        /// <summary>
        /// Default length of strings when they are passed to Dapper (or other ORM).
        /// Default is 4000, but any value larger than this field will not have the default value applied.
        /// </summary>
        public const int DefaultLength = 4000;

        /// <summary>
        /// Ansi vs Unicode 
        /// </summary>
        public bool IsAnsi { get; set; }

        /// <summary>
        /// Fixed length 
        /// </summary>
        public bool IsFixedLength { get; set; }

        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public int Length { get; set; }
        #endregion

        /// <summary>
        /// Create a new StringParameterInfo
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        public StringParameterInfo(string? name, object? value = null, ParameterDirection? direction = null) : base(name, value, direction)
        {
            Length = -1;
            IsAnsi = IsAnsiDefault;
            Value = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public StringParameterInfo() : base(null, null)
        {
            Length = -1;
            IsAnsi = IsAnsiDefault;
            Value = null;
        }


        /// <summary>
        /// Gets a string representation of this DbString.
        /// </summary>
        public override string ToString() =>
            $"StringParameterInfo (Value: '{Value}', Length: {Length}, IsAnsi: {IsAnsi}, IsFixedLength: {IsFixedLength})";

    }
}
