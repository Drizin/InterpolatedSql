using System.Data;

namespace InterpolatedSql
{
    /// <summary>
    /// Represents a Sql Parameter of any DbType.
    /// For Strings (where there are important differences like Ansi/Unicode which define if each character will ocuppy
    /// </summary>
    public class DbTypeParameterInfo : SqlParameterInfo
    {
        #region Members
        /// <summary>
        /// Parameters added through string interpolation usually do not need to define their DbType, and Dapper/ORM will automatically detect the correct type, <br />
        /// but it's possible to explicitly define the DbType (which Dapper/ORM will map to corresponding type in your database)
        /// </summary>
        public DbType? DbType { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define their Size, and Dapper/ORM will automatically detect the correct size, <br />
        /// but it's possible to explicitly define the size (usually for strings, where in some specific scenarios you can get better performance by passing the exact data type)
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define this, as Dapper/ORM will automatically calculate the correct value
        /// </summary>
        public byte? Precision { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define this, as Dapper/ORM will automatically calculate the correct value
        /// </summary>
        public byte? Scale { get; set; }
        #endregion

        #region ctors
        /// <summary>
        /// New DbTypeParameterInfo
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        public DbTypeParameterInfo(string? name, object? value = null, ParameterDirection? direction = null, DbType? dbType = null, int? size = null, byte? precision = null, byte? scale = null)
            : base(name, value, direction)
        {
            this.DbType = dbType;
            this.Size = size;
            this.Precision = precision;
            this.Scale = scale;
        }


        /// <summary>
        /// New Parameter
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        public DbTypeParameterInfo(string? name, object value, ParameterDirection? direction, DbType? dbType, int? size) : this(name, value, direction, dbType, size, null, null)
        {
        }

        #endregion
    }
}
