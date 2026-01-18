using Dapper;
using InterpolatedSql.Dapper.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// List of SQL Parameters that are passed to Dapper methods
    /// </summary>
    public class ParametersDictionary : Dictionary<string, SqlParameterInfo>, SqlMapper.IDynamicParameters, SqlMapper.IParameterCallbacks
    {
        #region Members
        private DynamicParameters? _dynamicParameters = null;
        #endregion

        #region ctors
        /// <inheritdoc cref="ParametersDictionary"/>
        public ParametersDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// Creates a <see cref="ParametersDictionary"/> built from Implicit Parameters (loaded from <see cref="IInterpolatedSql.SqlParameters" />)
        /// and Explicit Parameters (loaded from <see cref="IInterpolatedSql.ExplicitParameters"/>)
        public static ParametersDictionary LoadFrom(IInterpolatedSql sql) 
        {
            sql = (sql as ISqlEnricher)?.GetEnrichedSql() ?? sql;
            var parameters = new ParametersDictionary();
            //HashSet<string> parmNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //TODO: check for name clashes, rename as required

            Func<InterpolatedSqlParameter, int, string> calculateAutoParameterName = 
                (sql as IDapperSqlBuilder)?.Options?.CalculateAutoParameterName ??
                ((parm, pos) => InterpolatedSqlDapperOptions.DefaultOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(
                    parm, pos, InterpolatedSql.SqlBuilders.InterpolatedSqlBuilderOptions.DefaultOptions));

            for (int i = 0; i < sql.ExplicitParameters.Count; i++)
            {
                parameters.Add(sql.ExplicitParameters[i]);
            }

            for (int i = 0; i < sql.SqlParameters.Count; i++)
            {
                var parmName = calculateAutoParameterName(sql.SqlParameters[i], i);
                var parmValue = sql.SqlParameters[i].Argument;
                var format = sql.SqlParameters[i].Format;
                if (!string.IsNullOrWhiteSpace(format))
                    throw new ArgumentException("Unrecognized format modifier: " + format);

                if (parmValue is SqlParameterInfo parm)
                {
                    parm.Name = parmName;
                    parameters[parmName] = parm;
                }
                else
                    parameters.Add(new SqlParameterInfo(parmName, parmValue));
            }
            return parameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Convert the current parameters into Dapper Parameters, since Dapper will automagically set DbTypes, Sizes, etc, and map to target database
        /// 
        /// </summary>
        public virtual DynamicParameters DynamicParameters
        {
            // Most Dapper extensions work fine with a Dictionary{string, object}, 
            // but some methods like QueryMultiple (when used with Stored Procedures) may require DynamicParameters
            // TODO: should we just use DynamicParameters in all Dapper calls?
            get
            {
                if (_dynamicParameters == null)
                {
                    _dynamicParameters = new DynamicParameters();
                    foreach (var parameter in this.Values)
                        SqlParameterMapper.DefaultMapper.AddToDynamicParameters(_dynamicParameters, parameter);
                }
                return _dynamicParameters;
            }
        }
       
        /// <summary>
        /// Add a explicit parameter to this dictionary
        /// </summary>
        public void Add(SqlParameterInfo parameter)
        {
            this[parameter.Name!] = parameter;
        }

       
        /// <summary>
        /// Get parameter value
        /// </summary>
        public T? Get<T>(string key) => (T?)this[key].Value;

        /// <summary>
        /// Parameter Names
        /// </summary>
        public HashSet<string> ParameterNames => new HashSet<string>(this.Keys);

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            // IDynamicParameters is explicitly implemented (not public) - and it will add our dynamic paramaters to IDbCommand
            ((SqlMapper.IDynamicParameters)DynamicParameters).AddParameters(command, identity);
        }

        /// <summary>
        /// After Dapper command is executed, we should get output/return parameters back
        /// </summary>
        void SqlMapper.IParameterCallbacks.OnCompleted()
        {
            var dapperParameters = DynamicParameters;

            // Update output and return parameters back
            foreach (var oparm in this.Values.Where(p => p.ParameterDirection != ParameterDirection.Input && p.ParameterDirection != null))
            {
                oparm.Value = dapperParameters.Get<object>(oparm.Name!);
                oparm.OutputCallback?.Invoke(oparm.Value);
            }
        }
        #endregion
    }

}
