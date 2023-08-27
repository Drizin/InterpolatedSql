using System.Data;

namespace InterpolatedSql.Dapper.FluentQueryBuilder
{
    public interface IFluentQueryBuilder 
        : InterpolatedSql.FluentQueryBuilder.IFluentQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>, 
        IQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>, 
        IInterpolatedSqlBuilder<IFluentQueryBuilder, IDapperSqlCommand>, 
        IBuildable<IDapperSqlCommand>
    {
        //ParametersDictionary DapperParameters { get; }
        IDbConnection DbConnection { get; set; }

        IDapperSqlCommand Build();
    }
}