using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// IDapperSqlCommand are "commands ready to run" - this is where we extend those commands with Dapper facades
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.InterpolatedSqlBuilder"/> is the most generic builder - and is always ready to run.
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.QueryBuilder"/> is a builder with some helpers to build SELECT queries - and is always ready to run.
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder.FluentQueryBuilder"/> is a step-by-step fluent builder - it is ready to run only in some stages of the builder
    /// </summary>
    public static partial class IDapperSqlCommandExtensions
    {

        #region Dapper (IDapperSqlCommand.Execute())
        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static int Execute(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.Execute(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static Task<int> ExecuteAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (IDapperSqlCommand.ExecuteScalar())
        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as object.
        /// </summary>
        public static object ExecuteScalar(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteScalar(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as T.
        /// </summary>
        public static Task<T> ExecuteScalarAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteScalarAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as object.
        /// </summary>
        public static Task<object> ExecuteScalarAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteScalarAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (IDapperSqlCommand.QueryMultiple())
        /// <summary>
        /// Executes the query (using Dapper), returning multiple result sets, and access each in turn.
        /// </summary>
        public static SqlMapper.GridReader QueryMultiple(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            // DapperParameters because QueryMultiple with Stored Procedures doesn't work with Dictionary<string, object> - see https://github.com/DapperLib/Dapper/issues/1580#issuecomment-889813797
            return command.DbConnection.QueryMultiple(sql: command.Sql, param: ParametersDictionary.LoadFrom(command).DynamicParameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning multiple result sets, and access each in turn.
        /// </summary>
        public static Task<SqlMapper.GridReader> QueryMultipleAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            // DapperParameters because QueryMultiple with Stored Procedures doesn't work with Dictionary<string, object> - see https://github.com/DapperLib/Dapper/issues/1580#issuecomment-889813797
            return command.DbConnection.QueryMultipleAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command).DynamicParameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (IDapperSqlCommand.Query<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static IEnumerable<T> Query<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.Query<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirst<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirst<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirstOrDefault<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefault<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingle<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingle<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingleOrDefault<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefault<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.Query() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static IEnumerable<dynamic> Query(this IDapperSqlCommand command, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.Query(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirst(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirst(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirstOrDefault(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefault(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingle(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingle(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingleOrDefault(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefault(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.Query<object>())
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static IEnumerable<object> Query(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.Query(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirst(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirst(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirstOrDefault(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefault(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingle(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingle(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingleOrDefault(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefault(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<IEnumerable<T>> QueryAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstOrDefaultAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefaultAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleOrDefaultAsync<T>(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefaultAsync<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.QueryAsync() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<IEnumerable<dynamic>> QueryAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstOrDefaultAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefaultAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleOrDefaultAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefaultAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.QueryAsync<object>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<IEnumerable<object>> QueryAsync(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryAsync(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstAsync(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstAsync(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstOrDefaultAsync(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QueryFirstOrDefaultAsync(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleAsync(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleAsync(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleOrDefaultAsync(this IDapperSqlCommand command, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.QuerySingleOrDefaultAsync(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlCommand.ExecuteReader())
        /// <summary>
        /// Executes the query (using Dapper), returning an System.Data.IDataReader
        /// </summary>
        public static IDataReader ExecuteReader(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteReader(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning an System.Data.IDataReader
        /// </summary>
        public static Task<IDataReader> ExecuteReaderAsync(this IDapperSqlCommand command, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.DbConnection.ExecuteReaderAsync(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion


    }
}
