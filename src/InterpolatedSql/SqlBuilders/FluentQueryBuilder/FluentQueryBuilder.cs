using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// FluentQueryBuilder is like <see cref="QueryBuilder{U, RB, R}"/> (an injection-safe dynamic SQL builder focused in building dynamic WHERE filters)
    /// but using a Fluent API that helps to build the query step by step 
    /// (query is built using .Select(), .From(), .Where(), etc)
    /// </summary>
    /// <typeparam name="U">Builder Underlying type (implementing recursive generics)</typeparam>
    /// <typeparam name="RB">Builder type that builds the Return Type</typeparam>
    /// <typeparam name="R">Return Type</typeparam>
    public abstract class FluentQueryBuilder<U, RB, R> : QueryBuilder<U, RB,R>,
        IFluentQueryBuilder<U, RB, R>,
        IBuildable<R>
        where U : IFluentQueryBuilder<U, RB, R>, IQueryBuilder<U, RB, R>, IInterpolatedSqlBuilder<U, R>, IBuildable<R>
        where RB : IInterpolatedSqlBuilder<RB, R>
        where R: class, IInterpolatedSql
    {

        #region Members
        private int? _rowCount = null;
        private int? _offset = null;
        private bool _isSelectDistinct = false;
        #endregion

        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2) : base(combinedBuilderFactory1, combinedBuilderFactory2)
        {
        }

        /// <inheritdoc/>
        public FluentQueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2, 
            IDbConnection connection) : base(combinedBuilderFactory1, combinedBuilderFactory2)
        {
            DbConnection = connection;
        }
        #endregion

        #region Fluent API methods //TODO: should these be converted to explicit interface implementation?
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        public new ISelectBuilder<U, RB, R> Select(FormattableString column)
        {
            // Copy of base.Select(FormattableString)
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            _selects.AppendFormattableString(column);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        public ISelectBuilder<U, RB, R> Select(params FormattableString[] moreColumns)
        {
            //Select(column);
            foreach (var col in moreColumns)
                Select(col);
            return this;
        }

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<U, RB, R> SelectDistinct(FormattableString select)
        {
            _isSelectDistinct = true;
            // Copy of base.Select(FormattableString)
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            _selects.AppendFormattableString(select);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<U, RB, R> SelectDistinct(params FormattableString[] moreColumns)
        {
            //SelectDistinct(select);
            foreach (var col in moreColumns)
                SelectDistinct(col);
            return this;
        }

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        public IFromBuilder<U, RB, R> From(FormattableString from)
        {
            var target = InterpolatedSqlBuilderFactory.Default.Create();
            Options.Parser.ParseAppend(target, from);
            if (_froms.IsEmpty && !Regex.IsMatch(target.Format, "\\b FROM \\b", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                target.InsertLiteral(0, "FROM ");
            // Copy of base.From(FormattableString)
            _froms.Append(target.Build());
            _froms.AppendLiteral(NewLine); 
            return this;
        }
        //TODO: create options with InnerJoin, LeftJoin, RightJoin, FullJoin, CrossJoin? Create overloads with table alias?


        /// <summary>
        /// Adds one or more column(s) to groupby clauses.
        /// </summary>
        public IGroupByBuilder<U, RB, R> GroupBy(FormattableString groupBy)
        {
            // Copy of base.GroupBy(FormattableString)
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            _groupBy.AppendFormattableString(groupBy);
            return this;
        }

        /// <summary>
        /// Adds one or more condition(s) to having clauses.
        /// </summary>
        public IGroupByHavingBuilder<U, RB, R> Having(FormattableString having)
        {
            // Copy of base.Having(FormattableString)
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            _having.AppendFormattableString(having);
            return this;
        }

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        public IOrderByBuilder<U, RB, R> OrderBy(FormattableString orderBy)
        {
            // Copy of base.OrderBy(FormattableString)
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            _orderBy.AppendFormattableString(orderBy);
            return this;
        }

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        public ICompleteBuilder<U, RB, R> Limit(int offset, int rowCount)
        {
            _offset = offset;
            _rowCount = rowCount;
            return this;
        }

        #endregion

        #region Where overrides
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public new IWhereBuilder<U, RB, R> Where(Filter filter)
        {
            base.Where(filter);
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public new IWhereBuilder<U, RB, R> Where(Filters filters)
        {
            base.Where(filters);
            return this;
        }


        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        public new IWhereBuilder<U, RB, R> Where(FormattableString filter)
        {
            base.Where(filter);
            return this;
        }
        #endregion

        /// <inheritdoc/>
        public override R Build()
        {
            var cachedCombinedQuery = _combinedBuilderFactory1(Options);

            //TODO: except for some parts (like DISTINCT, *, OFFSET) this is very similar to base class QueryBuilder
            cachedCombinedQuery.AppendLiteral("SELECT ").AppendLiteral(_isSelectDistinct ? "DISTINCT " : "");
            if (_selects.IsEmpty)
                cachedCombinedQuery.AppendLiteral("*");
            else
                cachedCombinedQuery.Append(_selects.Build());



            if (!_froms.IsEmpty)
            {
                _froms.TrimEnd();
                cachedCombinedQuery.AppendLine(_froms.Build()); //TODO: inner join and left/outer join shortcuts?
                // TODO: AppendLine adds linebreak BEFORE the value - is that a little counterintuitive?
            }


            if (_filters.Any())
            {
                var filters = GetFilters()!;
                cachedCombinedQuery.AppendLine().AppendLiteral("WHERE ").Append(filters.Build());
            }

            if (!_groupBy.IsEmpty)
                cachedCombinedQuery.AppendLine().AppendLiteral("GROUP BY").Append(_groupBy.Build());
            if (!_having.IsEmpty)
                cachedCombinedQuery.AppendLine().AppendLiteral("HAVING ").Append(_having.Build());
            if (!_orderBy.IsEmpty)
                cachedCombinedQuery.AppendLine().AppendLiteral("ORDER BY ").Append(_orderBy.Build());
            if (_rowCount != null)
                cachedCombinedQuery.AppendLine().AppendLiteral("OFFSET ").AppendLiteral((_offset ?? 0).ToString())
                    .AppendLiteral($"ROWS FETCH NEXT {_rowCount} ROWS ONLY"); // TODO: PostgreSQL? "LIMIT row_count OFFSET offset"

            return cachedCombinedQuery.Build();
        }

        IInterpolatedSql IBuildable.Build()
        {
            return Build();
        }

    }

}