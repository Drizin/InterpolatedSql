using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace InterpolatedSql
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> (an injection-safe dynamic SQL builder where SqlParameters are defined using string interpolation),
    /// but with some extra helpers to dynamically build a list of WHERE Filters (which are later concatenated and will replace the keyword /**where**/),
    /// and can also dynamically define a list of tables (FROM) or columns (SELECT).
    /// </summary>
    public class QueryBuilder<T> : InterpolatedSqlBuilder<T>
        where T : QueryBuilder<T>
    {
        #region Members
        protected readonly InterpolatedSqlBuilder _selects = new InterpolatedSqlBuilder();
        protected readonly InterpolatedSqlBuilder _froms = new InterpolatedSqlBuilder();
        protected readonly Filters _filters = new Filters();
        protected readonly InterpolatedSqlBuilder _groupBy = new InterpolatedSqlBuilder();
        protected readonly InterpolatedSqlBuilder _having = new InterpolatedSqlBuilder();
        protected readonly InterpolatedSqlBuilder _orderBy = new InterpolatedSqlBuilder();
        protected InterpolatedSqlBuilder? _cachedCombinedQuery = null;

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
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        public QueryBuilder() : base()
        {
        }

        /// <summary>
        /// New empty QueryBuilder. <br />
        /// Query should be built using .Append(), .AppendLine(), or .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        public QueryBuilder(IDbConnection connection) : this()
        {
            DbConnection = connection;
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Query can be modified using .Append(), .AppendLine(), .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(FormattableString query) : base(query)
        {
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Query can be modified using .Append(), .AppendLine(), .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="SqlParameters"/>.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection connection, FormattableString query) : this(query)
        {
            DbConnection = connection;
        }
        #endregion

        #region Filters/Where
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual T Where(Filter filter)
        {
            _filters.Add(filter);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual T Where(Filters filters)
        {
            _filters.Add(filters);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }


        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically captured into <see cref="SqlParameters"/>.
        /// </summary>
        public virtual T Where(FormattableString filter)
        {
            return Where(new Filter(filter));
        }

        /// <summary>
        /// Writes the SQL Statement of all filter(s) (going recursively if there are nested filters) <br />
        /// Does NOT add leading "WHERE" keyword. <br />
        /// Returns null if no filter was defined.
        /// </summary>
        public InterpolatedSqlBuilder? GetFilters()
        {
            if (_filters == null || !_filters.Any())
                return null;

            InterpolatedSqlBuilder filters = new InterpolatedSqlBuilder();
            _filters.WriteTo(filters); // this writes all filters, going recursively if there are nested filters
            return filters;
        }
        #endregion

        #region IInterpolatedSql overrides

        #region CombinedQuery
       
        /// <summary>
        /// Gets the combined command
        /// </summary>
        public virtual InterpolatedSqlBuilder CombinedQuery
        {
            get
            {
                if (_cachedCombinedQuery != null)
                    return _cachedCombinedQuery;

                InterpolatedSqlBuilder combinedQuery;

                // An initial template may or may not have been provided
                if (_format.Length > 0)
                {
                    combinedQuery = new InterpolatedSqlBuilder(Options, new StringBuilder(_format.Length).Append(_format), _sqlParameters.ToList());
                }
                else
                    combinedQuery = new InterpolatedSqlBuilder(Options);

                if (_selects.Sql?.Length > 0)
                {
                    _selects.TrimEnd();

                    if (_selects.Sql?.Length > 0)
                    {
                        string matchKeyword;
                        int matchPos;
                        if (((matchKeyword = "/**select**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{select}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                        {
                            // Template has a Placeholder for SELECT
                            _selects.InsertLiteral(0, "SELECT ");
                            combinedQuery.Remove(matchPos, matchKeyword.Length);
                            combinedQuery.Insert(matchPos, _selects);
                        }
                        else if (((matchKeyword = "/**selects**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                                ((matchKeyword = "{selects}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                        {
                            // Template has a placeholder for SELECTS - which means that
                            // SELECT should be already in template and user just wants to add more columns using "selects" placeholder
                            _selects.InsertLiteral(0, ", ");
                            combinedQuery.Remove(matchPos, matchKeyword.Length);
                            combinedQuery.Insert(matchPos, _selects);
                        }
                    }
                }

                if (_froms.Sql?.Length > 0)
                {
                    _froms.TrimEnd();
                    string matchKeyword;
                    int matchPos;
                    if (((matchKeyword = "/**from**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                        ((matchKeyword = "{from}")     != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for FROMs
                        _froms.InsertLiteral(0, "FROM ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _froms);
                    }
                    else if (((matchKeyword = "/**joins**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{joins}")      != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a placeholder for JOINS (yeah - JOINS and FROMS are currently using same variable)
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _froms);
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
                        filters.InsertLiteral(0, "WHERE ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, filters);
                    }
                    else if (((matchKeyword = "/**filters**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                             ((matchKeyword = "{filters}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for Filters
                        filters.InsertLiteral(0, "AND ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, filters);
                    }
                    else
                    {
                        //TODO: if Query Template was provided, check if Template ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                        // else...
                        //TODO: if Query Template was provided, check if Template ends has WHERE with real conditions... set hasWhereConditions=true 
                        // else...
                        combinedQuery.Append(filters);
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
                        _groupBy.InsertLiteral(0, "GROUP BY ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _groupBy);
                    }
                    else if (((matchKeyword = "/**groupby_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                             ((matchKeyword = "{groupby_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for "adding more columns to" GROUP BY
                        _groupBy.InsertLiteral(0, ", ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _groupBy);
                    }
                    else
                    {
                        combinedQuery.AppendLiteral("GROUP BY ");
                        combinedQuery.Append(_groupBy);
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
                        _having.InsertLiteral(0, "HAVING ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _having);
                    }
                    else if (((matchKeyword = "/**having_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                             ((matchKeyword = "{having_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for "adding more columns to" HAVING
                        _having.InsertLiteral(0, ", ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _having);
                    }
                    else
                    {
                        combinedQuery.AppendLiteral("HAVING ");
                        combinedQuery.Append(_having);
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
                        _orderBy.InsertLiteral(0, "ORDER BY ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _orderBy);
                    }
                    else if (((matchKeyword = "/**orderby_additional**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                             ((matchKeyword = "{orderby_additional}") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for "adding more columns to" ORDER BY
                        _orderBy.InsertLiteral(0, ", ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _orderBy);
                    }
                    else
                    {
                        combinedQuery.AppendLiteral("ORDER BY ");
                        combinedQuery.Append(_orderBy);
                    }
                }


                _cachedCombinedQuery = combinedQuery;
                return _cachedCombinedQuery;
            }
        }
        #endregion

        /// <inheritdoc />
        public override IReadOnlyList<InterpolatedSqlParameter> SqlParameters => Initializing ? base.SqlParameters : CombinedQuery.SqlParameters;

        /// <inheritdoc />
        public override string Format => Initializing ? base.Format : CombinedQuery.Format;

        /// <inheritdoc />
        protected override void ClearLiteralCache()
        {
            _cachedCombinedQuery = null;
            base.ClearLiteralCache();
        }

        #endregion

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual T From(ref InterpolatedSqlHandler value)
        {
            _froms.Append(value.InterpolatedSqlBuilder);
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }
#endif

        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual T From(LegacyFormattableString fromString)
        {
            _froms.Append(fromString);
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual T Select(ref InterpolatedSqlHandler value)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            if (value.InterpolatedSqlBuilder.SqlParameters.Count == 0) // if it's just a plain string, then it's code (not user input) - so it's safe // TODO: review this! should we always get strings here? what about SELECT [expression] ? allow both?
                _selects.AppendLiteral(value.InterpolatedSqlBuilder.Format);
            else
                _selects.Append(value.InterpolatedSqlBuilder);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }
#endif

        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual T Select(LegacyFormattableString selectString)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            if (((FormattableString)selectString).ArgumentCount == 0) // if it's just a plain string, then it's code (not user input) - so it's safe // TODO: review this! should we always get strings here? what about SELECT [expression] ? allow both?
                _selects.AppendLiteral(((FormattableString)selectString).Format);
            else
                _selects.Append(selectString);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the GROUP BY clause.
        /// </summary>
        public virtual T GroupBy(ref InterpolatedSqlHandler value)
        {
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            if (value.InterpolatedSqlBuilder.SqlParameters.Count == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _groupBy.AppendLiteral(value.InterpolatedSqlBuilder.Format);
            else
                _groupBy.Append(value.InterpolatedSqlBuilder);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }
#endif

        /// <summary>
        /// Adds a new column to the GROUP BY clause.
        /// </summary>
        public virtual T GroupBy(LegacyFormattableString selectString)
        {
            if (!_groupBy.IsEmpty)
                _groupBy.AppendLiteral(", ");
            if (((FormattableString)selectString).ArgumentCount == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _groupBy.AppendLiteral(((FormattableString)selectString).Format);
            else
                _groupBy.Append(selectString);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the HAVING clause.
        /// </summary>
        public virtual T Having(ref InterpolatedSqlHandler value)
        {
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            if (value.InterpolatedSqlBuilder.SqlParameters.Count == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _having.AppendLiteral(value.InterpolatedSqlBuilder.Format);
            else
                _having.Append(value.InterpolatedSqlBuilder);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }
#endif

        /// <summary>
        /// Adds a new column to the HAVING clause.
        /// </summary>
        public virtual T Having(LegacyFormattableString selectString)
        {
            if (!_having.IsEmpty)
                _having.AppendLiteral(", ");
            if (((FormattableString)selectString).ArgumentCount == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _having.AppendLiteral(((FormattableString)selectString).Format);
            else
                _having.Append(selectString);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }


#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the ORDER BY clause.
        /// </summary>
        public virtual T OrderBy(ref InterpolatedSqlHandler value)
        {
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            if (value.InterpolatedSqlBuilder.SqlParameters.Count == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _orderBy.AppendLiteral(value.InterpolatedSqlBuilder.Format);
            else
                _orderBy.Append(value.InterpolatedSqlBuilder);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }
#endif

        /// <summary>
        /// Adds a new column to the ORDER BY clause.
        /// </summary>
        public virtual T OrderBy(LegacyFormattableString selectString)
        {
            if (!_orderBy.IsEmpty)
                _orderBy.AppendLiteral(", ");
            if (((FormattableString)selectString).ArgumentCount == 0) // if it's just a plain string, then it's programmatic (not user input) so it's safe
                _orderBy.AppendLiteral(((FormattableString)selectString).Format);
            else
                _orderBy.Append(selectString);
            ClearLiteralCache();
            ClearParametersCache();
            return (T)this;
        }


    }

    /// <inheritdoc/>
    public class QueryBuilder : QueryBuilder<QueryBuilder>
    {
        #region ctors
        /// <inheritdoc/>
        public QueryBuilder() : base()
        {
        }
        
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : base(connection)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(FormattableString query) : base(query)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(connection, query)
        {
        }
        #endregion
    }

}
