using Dapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// List of SQL Parameters that are passed to Dapper methods
    /// </summary>
    public class ParametersDictionary : Dictionary<string, SqlParameterInfo>, SqlMapper.IDynamicParameters
    {
        #region Members
        private DynamicParameters? _dynamicParameters = null;
        #endregion

        #region ctors
        /// <inheritdoc cref="ParametersDictionary"/>
        public ParametersDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Convert the current parameters into Dapper Parameters, since Dapper will automagically set DbTypes, Sizes, etc, and map to target database
        /// </summary>
        public virtual DynamicParameters DynamicParameters
        {
            // Most Dapper extensions would work fine with a Dictionary{string, object}
            // but some methods like QueryMultiple (when used with Stored Procedures) may require DynamicParameters
            // Our extension methods will all use DynamicParameters in all Dapper calls
            // If you already have your own DynamicParameters you can also append new parameters to it from this ParametersDictionary like example below
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

        #endregion
    }

}
