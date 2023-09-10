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


        /// <summary>
        /// Output parameters (created using <see cref="ConfigureOutputParameter{T, TP}(T, Expression{Func{T, TP}}, OutputParameterDirection)"/>) invoke this callback to set their value back to Target type
        /// </summary>
        public Action<object>? OutputCallback { get; set; }
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


        /// <summary>
        /// Creates a new Output Parameter (can be Output, InputOutput, or ReturnValue) <br />
        /// and registers a callback action which (after command invocation) will populate back parameter output value into an instance property.
        /// </summary>
        /// <param name="target">Target variable where output value will be set.</param>
        /// <param name="expression">Property where output value will be set. If it's InputOutput type this value will be passed.</param>
        /// <param name="direction">The type of output of the parameter.</param>
        public void ConfigureOutputParameter<T, TP>(T target, Expression<Func<T, TP>> expression, OutputParameterDirection direction = OutputParameterDirection.Output)
        {
            if (ParameterDirection != null)
                throw new ArgumentException($"For Output-type parameters the ParameterDirection should only be set by {nameof(ConfigureOutputParameter)}");
            
            ParameterDirection = (ParameterDirection)direction;

            // For InputOutput we send current value
            if (ParameterDirection == System.Data.ParameterDirection.InputOutput)
            {
                if (Value != null)
                    throw new ArgumentException($"For {nameof(OutputParameterDirection.InputOutput)} the value should be set by the target expression");
                Value = expression.Compile().Invoke(target);
            }

            var setter = GetSetter(expression);
            OutputCallback = new Action<object>(o => {
                TP val;
                if (o is TP)
                    val = (TP)o;
                else
                {
                    try
                    {
                        val = (TP)Convert.ChangeType(o, typeof(TP));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Can't convert {Name} ({Value}) to type {typeof(TP).Name}", ex);
                    }
                }
                setter(target, val); // TP (property type) must match the return value
            });
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
        #endregion

    }
}
