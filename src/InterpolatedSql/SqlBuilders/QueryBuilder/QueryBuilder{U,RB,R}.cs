using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterpolatedSql.SqlBuilders
{
    /// <summary>
    /// QueryBuilder extends InterpolatedSqlBuilder (so it's a dynamic SQL builder where SqlParameters are defined using string interpolation and yet it's but injection safe),
    /// but it adds some extra helpers to dynamically build a list of WHERE Filters (which are later concatenated and will replace the keyword /**where**/),
    /// and can also dynamically define a list of tables (FROM) or columns (SELECT).
    /// </summary>
    public abstract class QueryBuilder<U, RB, R> : SqlBuilder<U, R>, IBuildable<R>, IQueryBuilder<U, RB, R>, ISqlEnricher
        where U : IQueryBuilder<U, RB, R>, ISqlBuilder<U, R>, IBuildable<R>
        where RB : IInterpolatedSqlBuilderBase, IBuildable<R>
        where R : class, IInterpolatedSql
    {
        #region Members
        protected readonly IInterpolatedSqlBuilderBase _selects = SqlBuilderFactory.Default.Create();
        protected readonly IInterpolatedSqlBuilderBase _froms = SqlBuilderFactory.Default.Create();
        protected readonly Filters _filters = new Filters();
        protected readonly IInterpolatedSqlBuilderBase _groupBy = SqlBuilderFactory.Default.Create();
        protected readonly IInterpolatedSqlBuilderBase _having = SqlBuilderFactory.Default.Create();
        protected readonly IInterpolatedSqlBuilderBase _orderBy = SqlBuilderFactory.Default.Create();
        protected readonly Func<InterpolatedSqlBuilderOptions?, RB> _combinedBuilderFactory1;
        protected readonly Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> _combinedBuilderFactory2;
        /// <summary>
        /// How a list of Filters are combined (AND operator or OR operator)
        /// </summary>
        public Filters.FiltersType FiltersType
        {
            get { return _filters.Type; }
            set { _filters.Type = value; }
        }
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. <br />
        /// Query should be built using .Append(), .AppendLine(), or .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="InterpolatedSqlBuilderBase.SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        protected QueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2
            ) : base()
        {
            _combinedBuilderFactory1 = combinedBuilderFactory1;
            _combinedBuilderFactory2 = combinedBuilderFactory2;
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Query can be modified using .Append(), .AppendLine(), .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="InterpolatedSqlBuilderBase.SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        protected QueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2,
            FormattableString query) : base(query)
        {
            _combinedBuilderFactory1 = combinedBuilderFactory1;
            _combinedBuilderFactory2 = combinedBuilderFactory2;
        }
        #endregion

        #region Filters/Where
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual U Where(Filter filter)
        {
            _filters.Add(filter);
            return (U)(object)this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual U Where(Filters filters)
        {
            _filters.Add(filters);
            return (U)(object)this;
        }


        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="InterpolatedSqlBuilderBase.SqlParameters"/>.
        /// </summary>
        public virtual U Where(FormattableString filter)
        {
            return Where(new Filter(filter));
        }

        /// <summary>
        /// Writes the SQL Statement of all filter(s) (going recursively if there are nested filters) <br />
        /// Does NOT add leading "WHERE" keyword. <br />
        /// Returns null if no filter was defined.
        /// </summary>
        public IInterpolatedSqlBuilderBase? GetFilters()
        {
            if (_filters == null || !_filters.Any())
                return null;

            IInterpolatedSqlBuilderBase filters = SqlBuilderFactory.Default.Create();
            _filters.WriteTo(filters); // this writes all filters, going recursively if there are nested filters
            return filters;
        }
        #endregion

        IInterpolatedSql ISqlEnricher.GetEnrichedSql()
        {
            return Build();
        }

        /// <inheritdoc/>
        public override R Build()
        {
            RB combinedQuery;
            if (_combinedBuilderFactory1 == null || _combinedBuilderFactory2 == null)
                return null!; // initializing

            // An initial template may or may not have been provided
            if (base.IsEmpty)
                combinedQuery = _combinedBuilderFactory1(Options);
            else
                combinedQuery = _combinedBuilderFactory2(Options, new StringBuilder(Format), SqlParameters.ToList());

            _selects.TrimEnd();
            if (!_selects.IsEmpty)
            {
                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**select**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{select}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for SELECT
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _selects.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "SELECT ");
                }
                else if (((matchKeyword = "/**selects**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                        ((matchKeyword = "{selects}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a placeholder for SELECTS - which means that
                    // SELECT should be already in template and user just wants to add more columns using "selects" placeholder
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _selects.AsSql());
                    if (!_selects.ToString().StartsWith(", "))
                        combinedQuery.InsertLiteral(matchPos, ", ");
                }
            }

            _froms.TrimEnd();
            if (!_froms.IsEmpty)
            {
                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**from**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{from}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for FROMs
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _froms.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "FROM ");
                }
                else if (((matchKeyword = "/**joins**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                        ((matchKeyword = "{joins}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a placeholder for JOINS (yeah - JOINS and FROMS are currently using same variable)
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _froms.AsSql());
                }
            }

            if (_filters.Any())
            {
                var filters = GetFilters()!;

                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**where**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{where}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for Filters
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, filters.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "WHERE ");
                }
                else if (((matchKeyword = "/**filters**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{filters}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for Filters
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, filters.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "AND ");
                }
                else
                {
                    //TODO: if Query Template was provided, check if Template ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                    // else...
                    //TODO: if Query Template was provided, check if Template ends has WHERE with real conditions... set hasWhereConditions=true 
                    // else...
                    combinedQuery.AppendLiteral("WHERE ");
                    combinedQuery.Append(filters.AsSql());
                }
            }

            if (!_groupBy.IsEmpty)
            {
                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**groupby**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{groupby}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for GROUP BY
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _groupBy.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "GROUP BY ");
                }
                else if (((matchKeyword = "/**groupby_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{groupby_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for "adding more columns to" GROUP BY
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _groupBy.AsSql());
                    combinedQuery.InsertLiteral(matchPos, ", ");
                }
                else
                {
                    combinedQuery.AppendLiteral("GROUP BY ");
                    combinedQuery.Append(_groupBy.AsSql());
                }
            }

            if (!_having.IsEmpty)
            {
                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**having**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{having}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for HAVING
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _having.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "HAVING ");
                }
                else if (((matchKeyword = "/**having_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{having_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for "adding more columns to" HAVING
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _having.AsSql());
                    combinedQuery.InsertLiteral(matchPos, ", ");
                }
                else
                {
                    combinedQuery.AppendLiteral("HAVING ");
                    combinedQuery.Append(_having.AsSql());
                }
            }

            if (!_orderBy.IsEmpty)
            {
                string matchKeyword;
                int matchPos;
                if (((matchKeyword = "/**orderby**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                    ((matchKeyword = "{orderby}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for ORDER BY
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _orderBy.AsSql());
                    combinedQuery.InsertLiteral(matchPos, "ORDER BY ");
                }
                else if (((matchKeyword = "/**orderby_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{orderby_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                {
                    // Template has a Placeholder for "adding more columns to" ORDER BY
                    combinedQuery.Remove(matchPos, matchKeyword.Length);
                    combinedQuery.Insert(matchPos, _orderBy.AsSql());
                    combinedQuery.InsertLiteral(matchPos, ", ");
                }
                else
                {
                    combinedQuery.AppendLiteral("ORDER BY ");
                    combinedQuery.Append(_orderBy.AsSql());
                }
            }

            return combinedQuery.Build();
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual U From(ref InterpolatedSqlHandler value)
        {
            _froms.Append(value.InterpolatedSqlBuilder.AsSql());
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual U From(FormattableString fromString)
        {
            _froms.Append(fromString);
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            return (U)(object)this;
        }
#endif

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual U Select(ref InterpolatedSqlHandler value)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            _selects.Append(value.InterpolatedSqlBuilder.AsSql());
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual U Select(FormattableString selectString)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            _selects.Append(selectString);
            return (U)(object)this;
        }
#endif

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the GROUP BY clause.
        /// </summary>
        public virtual U GroupBy(ref InterpolatedSqlHandler value)
        {
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            _groupBy.Append(value.InterpolatedSqlBuilder.AsSql());
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Adds a new column to the GROUP BY clause.
        /// </summary>
        public virtual U GroupBy(FormattableString groupBy)
        {
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            _groupBy.Append(groupBy);
            return (U)(object)this;
        }
#endif

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the HAVING clause.
        /// </summary>
        public virtual U Having(ref InterpolatedSqlHandler value)
        {
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            _having.Append(value.InterpolatedSqlBuilder.AsSql());
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Adds a new column to the HAVING clause.
        /// </summary>
        public virtual U Having(FormattableString having)
        {
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            _having.Append(having);
            return (U)(object)this;
        }
#endif


#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the ORDER BY clause.
        /// </summary>
        public virtual U OrderBy(ref InterpolatedSqlHandler value)
        {
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            _orderBy.Append(value.InterpolatedSqlBuilder.AsSql());
            return (U)(object)this;
        }
#else
        /// <summary>
        /// Adds a new column to the ORDER BY clause.
        /// </summary>
        public virtual U OrderBy(FormattableString orderBy)
        {
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            _orderBy.Append(orderBy);
            return (U)(object)this;
        }
#endif


    }

}
