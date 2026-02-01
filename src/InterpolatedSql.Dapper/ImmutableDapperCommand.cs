using Dapper;
using System.Collections.Generic;
using System.Data;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Immutable implementation of <see cref="IDapperSqlCommand"/>.
    /// </summary>

    public class ImmutableDapperCommand : ImmutableInterpolatedSql, IDapperSqlCommand
    {
        /// <summary>
        /// Database connection associated to the command
        /// </summary>
        public IDbConnection DbConnection { get; set; }

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DictionaryParameters { get; }

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public DynamicParameters DapperParameters { get; }

        /// <inheritdoc />
        public ImmutableDapperCommand(IDbConnection connection,
            string sql, string format, IReadOnlyList<InterpolatedSqlParameter> sqlParameters, IReadOnlyList<SqlParameterInfo> explicitParameters, ParametersDictionary dictionaryParameters, DynamicParameters dapperParameters) : base(sql, format, sqlParameters, explicitParameters)
        {
            DbConnection = connection;
            DictionaryParameters = dictionaryParameters;
            DapperParameters = dapperParameters;
        }
    }
}
