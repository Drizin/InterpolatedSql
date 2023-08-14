using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace InterpolatedSql.FluentQueryBuilder
{
    /// <summary>
    /// FluentQueryBuilder is like <see cref="QueryBuilder{T}"/> (an injection-safe dynamic SQL builder focused in building dynamic WHERE filters)
    /// but using a Fluent API that helps to build the query step by step 
    /// (query is built using .Select(), .From(), .Where(), etc)
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FluentQueryBuilder<T>
        : QueryBuilder<T>, //TODO: should we inherit directly from InterpolatedSqlBuilder<T> and duplicate code from QueryBuilder<T>?
        IFluentQueryBuilder<T>
    
        where T : QueryBuilder<T>, IFluentQueryBuilder<T>
    {

        #region Members
        private readonly InterpolatedSqlBuilder _groupBy = new InterpolatedSqlBuilder();
        private readonly InterpolatedSqlBuilder _having = new InterpolatedSqlBuilder();
        private readonly InterpolatedSqlBuilder _orderBy = new InterpolatedSqlBuilder();
        private int? _rowCount = null;
        private int? _offset = null;
        private bool _isSelectDistinct = false;
        #endregion

        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder() : base()
        {
        }

        /// <inheritdoc/>
        public FluentQueryBuilder(IDbConnection connection) : base(connection)
        {
        }
        #endregion

        #region Fluent API methods //TODO: should these be converted to explicit interface implementation?
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        public ISelectBuilder<T> Select(FormattableString column)
        {
            base.Select(column);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        public ISelectBuilder<T> Select(params FormattableString[] moreColumns)
        {
            //Select(column);
            foreach (var col in moreColumns)
                Select(col);
            return this;
        }

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<T> SelectDistinct(FormattableString select)
        {
            _isSelectDistinct = true;
            base.Select(select);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<T> SelectDistinct(params FormattableString[] moreColumns)
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
        public IFromBuilder<T> From(FormattableString from)
        {
            var target = new InterpolatedSqlBuilder();
            base.Options.Parser.ParseAppend(from, target);
            if (_froms.IsEmpty && !Regex.IsMatch(target.Sql, "\\b FROM \\b", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                target.InsertLiteral(0, "FROM ");
            base.From((FormattableString)target);
            return this;
        }
        //TODO: create options with InnerJoin, LeftJoin, RightJoin, FullJoin, CrossJoin? Create overloads with table alias?


        /// <summary>
        /// Adds one or more column(s) to groupby clauses.
        /// </summary>
        public IGroupByBuilder<T> GroupBy(FormattableString groupBy)
        {
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            _groupBy.Append(groupBy);
            return this;
        }

        /// <summary>
        /// Adds one or more condition(s) to having clauses.
        /// </summary>
        public IGroupByHavingBuilder<T> Having(FormattableString having)
        {
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            _having.Append(having);
            return this;
        }

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        public IOrderByBuilder<T> OrderBy(FormattableString orderBy)
        {
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            _orderBy.Append(orderBy);
            return this;
        }

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        public ICompleteBuilder<T> Limit(int offset, int rowCount)
        {
            _offset = offset;
            _rowCount = rowCount;
            return (ICompleteBuilder<T>)this;
        }

        #endregion

        #region Where overrides
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public new IWhereBuilder<T> Where(Filter filter)
        {
            base.Where(filter);
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public new IWhereBuilder<T> Where(Filters filters)
        {
            base.Where(filters);
            return this;
        }


        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        public new IWhereBuilder<T> Where(FormattableString filter)
        {
            base.Where(filter);
            return this;
        }
        #endregion

        #region IInterpolatedSql overrides

        #region CombinedQuery

        /// <summary>
        /// Gets the combined command
        /// </summary>
        public override InterpolatedSqlBuilder CombinedQuery
        {
            get
            {
                if (_cachedCombinedQuery != null)
                    return _cachedCombinedQuery;

                _cachedCombinedQuery = new InterpolatedSqlBuilder(Options);

                _cachedCombinedQuery.AppendLiteral("SELECT ").AppendLiteral(_isSelectDistinct ? "DISTINCT " : "");
                if (_selects.IsEmpty)
                    _cachedCombinedQuery.AppendLiteral("*");
                else
                    _cachedCombinedQuery.Append(_selects);



                if (!_froms.IsEmpty)
                {
                    _froms.TrimEnd();
                    _cachedCombinedQuery.AppendLine(_froms); //TODO: inner join and left/outer join shortcuts?
                    // TODO: AppendLine adds linebreak BEFORE the value - is that a little counterintuitive?
                }


                if (_filters.Any())
                {
                    var filters = GetFilters()!;

                    _cachedCombinedQuery.AppendLine().AppendLiteral("WHERE ").Append(filters);
                }

                if (!_groupBy.IsEmpty)
                    _cachedCombinedQuery.AppendLine().AppendLiteral("GROUP BY").Append(_groupBy);
                if (!_having.IsEmpty)
                    _cachedCombinedQuery.AppendLine().AppendLiteral("HAVING ").Append(_having);
                if (!_orderBy.IsEmpty)
                    _cachedCombinedQuery.AppendLine().AppendLiteral("ORDER BY ").Append(_orderBy);
                if (_rowCount != null)
                    _cachedCombinedQuery.AppendLine().AppendLiteral("OFFSET ").AppendLiteral((_offset ?? 0).ToString())
                        .AppendLiteral($"ROWS FETCH NEXT {_rowCount} ROWS ONLY"); // TODO: PostgreSQL? "LIMIT row_count OFFSET offset"

                return _cachedCombinedQuery;
            }
        }
        #endregion

        #endregion


    }

}