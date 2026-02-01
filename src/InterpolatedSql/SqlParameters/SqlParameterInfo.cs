using InterpolatedSql.SqlBuilders;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace InterpolatedSql
{
    /// <summary>
    /// Generic wrapper to store information about SQL Parameters.
    /// Holds the underlying Value, parameter Name, and fields to support output parameters.
    /// This class should be mapped into something compatible with your database or ORM
    /// </summary>
    [DebuggerDisplay("{Name,nq} = {Value,nq}")]
    public class SqlParameterInfo
    {
        #region Members
        /// <summary>
        /// This is only used for Explicit Parameters (explicitly defined and stored in <see cref="InterpolatedSqlBuilderBase.ExplicitParameters"/>),
        /// and should NOT contain database-specific prefixes like "@" or ":".
        /// For Implicit Parameters (captured through string interpolation and stored in <see cref="InterpolatedSqlBuilderBase.SqlParameters"/>) the names
        /// are automatically calculated using <see cref="InterpolatedSqlBuilderBase.CalculateAutoParameterName(InterpolatedSqlParameter, int)"/>
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Value of parameter
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Parameters added through string interpolation are usually input parameters (passed from C# to SQL), <br />
        /// but you may explicitly describe parameters as Output, InputOutput, or ReturnValues.
        /// </summary>
        public ParameterDirection? ParameterDirection { get; set; }
        #endregion

        #region ctors
        /// <summary>
        /// New Parameter
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        public SqlParameterInfo(string? name, object? value = null, ParameterDirection? direction = null)
        {
            this.Name = name;
            this.Value = value;
            this.ParameterDirection = direction;
        }
        #endregion

        #region Enums
        /// <summary>
        /// Type of Output
        /// </summary>
        public enum OutputParameterDirection
        {
            /// <summary>
            /// The parameter is an output parameter.
            /// </summary>
            Output = 2,

            /// <summary>
            /// The parameter is capable of both input and output.
            /// </summary>
            InputOutput = 3,

            /// <summary>
            /// The parameter represents a return value from an operation such as a stored procedure, built-in function, or user-defined function.
            /// </summary>
            ReturnValue = 6
        }
        #endregion

        #region Methods
        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// </summary>
        private static Action<T, TProperty> GetSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod()!;

            var parameterT = Expression.Parameter(typeof(T), "x");
            var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

            var newExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Call(parameterT, setMethod, parameterTProperty),
                    parameterT,
                    parameterTProperty
                );

            return newExpression.Compile();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            SqlParameterInfo other = (obj as SqlParameterInfo)!;
            if (other == null) return false;

            if (Name != other.Name)
                return false;
            if (Value != other.Value)
                return false;
            if (ParameterDirection != other.ParameterDirection)
                return false;
            return true;
        }
        #endregion

    }
}
