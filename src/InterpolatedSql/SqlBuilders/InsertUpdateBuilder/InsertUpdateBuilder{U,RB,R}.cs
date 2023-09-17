using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpolatedSql.SqlBuilders.InsertUpdateBuilder
{
    /// <summary>
    /// InsertUpdateBuilder expects a list of columns and their respective values, and can build either a INSERT or an UPDATE statement.
    /// </summary>
    public abstract class InsertUpdateBuilder<U, RB, R> : SqlBuilder<U, R>, IBuildable<R>, IInsertUpdateBuilder<U, RB, R>
        where U : IInsertUpdateBuilder<U, RB, R>, ISqlBuilder<U, R>, IBuildable<R>
        where RB : IInterpolatedSqlBuilderBase, IBuildable<R>
        where R : class, IInterpolatedSql
    {
        public class ColumnValue
        {
            public string ColumnName { get; set; }
            public IInterpolatedSql Value { get; set; }
            public bool IncludeInInsert { get; set; }
            public bool IncludeInUpdate { get; set; }
        }
        #region Members
        protected readonly string _tableName;
        protected readonly List<ColumnValue>  _columnValues = new();
        protected readonly Func<InterpolatedSqlBuilderOptions?, RB> _combinedBuilderFactory;
        #endregion

        #region ctors
        /// <summary>
        /// New empty InsertUpdateBuilder. <br />
        /// Query should be built using .Append(), .AppendLine(), or .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="InterpolatedSqlBuilderBase.SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        protected InsertUpdateBuilder(string tableName, Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory) : base()
        {
            _tableName = tableName;
            _combinedBuilderFactory = combinedBuilderFactory;
        }
        #endregion

        /// <summary>
        /// Registers a column and the respective value
        /// </summary>
        public U AddColumn(string columnName, object value, bool includeInInsert = true, bool includeInUpdate = true)
        {
            var builder = SqlBuilderFactory.Default.Create();
            builder.AppendArgument(value);
            _columnValues.Add(new ColumnValue()
            {
                ColumnName = columnName,
                Value = builder.AsSql(),
                IncludeInInsert = includeInInsert,
                IncludeInUpdate = includeInUpdate
            });
            return (U)(object)this;
        }
#if NET6_0_OR_GREATER
        /// <summary>
        /// Registers a column and the respective value
        /// </summary>
        public U AddColumn(string columnName, ref InterpolatedSqlHandler value, bool includeInInsert = true, bool includeInUpdate = true)
        {
            _columnValues.Add(new ColumnValue()
            {
                ColumnName = columnName,
                Value = value.InterpolatedSqlBuilder.AsSql(),
                IncludeInInsert = includeInInsert,
                IncludeInUpdate = includeInUpdate
            });
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Registers a column and the respective value
        /// </summary>
        public U AddColumn(string columnName, FormattableString value, bool includeInInsert = true, bool includeInUpdate = true)
        {
            _columnValues.Add(new ColumnValue()
            {
                ColumnName = columnName,
                Value = SqlBuilderFactory.Default.Create(value).AsSql(),
                IncludeInInsert = includeInInsert,
                IncludeInUpdate = includeInUpdate
            }); ;
            return (U)(object)this;
        }
#endif

        /// <inheritdoc/>
        public virtual R GetInsertSql()
        {
            RB combinedQuery;
            if (_combinedBuilderFactory == null)
                return null!; // initializing

            var cols = _columnValues.Where(x => x.IncludeInInsert).ToList();
            combinedQuery = _combinedBuilderFactory(InterpolatedSqlBuilderOptions.DefaultOptions);
            combinedQuery.AppendLiteral("INSERT INTO " + _tableName + " (");
            for (int i = 0; i < cols.Count(); i++)
            {
                if (i > 0)
                    combinedQuery.AppendLiteral(", ");
                combinedQuery.AppendLiteral(cols[i].ColumnName);
            }
            combinedQuery.AppendLiteral(") VALUES (");
            for (int i = 0; i < cols.Count(); i++)
            {
                if (i > 0)
                    combinedQuery.AppendLiteral(", ");
                combinedQuery.Append(cols[i].Value);
            }
            combinedQuery.AppendLiteral(");");

            return combinedQuery.Build();
        }

        /// <inheritdoc/>
        public virtual R GetUpdateSql(
#if NET6_0_OR_GREATER
            ref InterpolatedSqlHandler whereCondition
#else
            FormattableString whereCondition
#endif
            )
        {
            RB combinedQuery;
            if (_combinedBuilderFactory == null)
                return null!; // initializing

            var cols = _columnValues.Where(x => x.IncludeInUpdate).ToList();
            combinedQuery = _combinedBuilderFactory(InterpolatedSqlBuilderOptions.DefaultOptions);
            combinedQuery.AppendLiteral("UPDATE " + _tableName + " SET ");
            for (int i = 0; i < cols.Count(); i++)
            {
                if (i > 0)
                    combinedQuery.AppendLiteral(", ");
                combinedQuery.AppendLiteral(cols[i].ColumnName);
                combinedQuery.AppendLiteral("=");
                combinedQuery.Append(cols[i].Value);
            }

#if NET6_0_OR_GREATER
            if (whereCondition.InterpolatedSqlBuilder.Format.Length > 0 && !char.IsWhiteSpace(whereCondition.InterpolatedSqlBuilder.Format[0]))
                combinedQuery.AppendLiteral(" ");
            if (!whereCondition.InterpolatedSqlBuilder.Format.Trim().StartsWith("WHERE"))
                combinedQuery.AppendLiteral("WHERE ");
            combinedQuery.Append(whereCondition.InterpolatedSqlBuilder.AsSql());
#else
            if (whereCondition.Format.Length > 0 && !char.IsWhiteSpace(whereCondition.Format[0]))
                combinedQuery.AppendLiteral(" ");
            if (!whereCondition.Format.Trim().StartsWith("WHERE"))
                combinedQuery.AppendLiteral("WHERE ");
            combinedQuery.Append(whereCondition);
#endif

            return combinedQuery.Build();
        }

        public override R Build()
        {
            throw new NotImplementedException("Should use GetUpdateSql() or GetInsertSql()");
        }
    }
}
