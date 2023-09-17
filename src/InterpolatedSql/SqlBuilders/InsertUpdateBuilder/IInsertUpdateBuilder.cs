using System;

namespace InterpolatedSql.SqlBuilders.InsertUpdateBuilder
{
    /// <inheritdoc cref="InsertUpdateBuilder{U, RB, R}"/>
    public interface IInsertUpdateBuilder<U, RB, R> : IBuildable<R>
        where U : IInsertUpdateBuilder<U, RB, R>, IBuildable<R>
        where RB : IInterpolatedSqlBuilderBase, IBuildable<R>
        where R : class, IInterpolatedSql
    {
        U AddColumn(string columnName, object value, bool includeInInsert = true, bool includeInUpdate = true);
#if NET6_0_OR_GREATER
        U AddColumn(string columnName, ref InterpolatedSqlHandler value, bool includeInInsert = true, bool includeInUpdate = true);
        R GetUpdateSql(ref InterpolatedSqlHandler whereCondition);
#else
        U AddColumn(string columnName, FormattableString value, bool includeInInsert = true, bool includeInUpdate = true);
        R GetUpdateSql(FormattableString whereCondition);
#endif
        R GetInsertSql();
    }
}