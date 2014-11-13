using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.DataAccess.Querying.Criteria;
using MicroORM.DataAccess.Querying.Criteria.Impls;
using MicroORM.DataAccess.Querying.GroupBy;
using MicroORM.DataAccess.Querying.Joins;
using MicroORM.DataAccess.Querying.Joins.Impl;
using MicroORM.DataAccess.Querying.OrderBy;
using MicroORM.DataAccess.Querying.Selects;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
    public class Query<TParentEntity> : IQuery<TParentEntity> where TParentEntity : class, new()
    {
        private readonly IDbConnection _connection;
        private readonly IDialect _dialect;
        private readonly IEnvironmentSettings _environment;
        private readonly List<ICriteriaRestriction> _criteriaRestrictions;
        private readonly IDictionary<Type, string> _eagerFetchProperties;
        private readonly List<IGroupByOption> _groupingOptions;
        private readonly IHydrator _hydrator;
        private readonly List<IJoinAction> _joinActions = new List<IJoinAction>();
        private readonly IMetadataStore _metadatastore;
        private readonly List<IOrderOption> _orderOptions;
        private readonly Type _parentEntity;
        private readonly HashSet<QueryParameter> _queryParameters;
        private readonly List<ISelectOption> _selectionOptions;
        private string _currentStatement;

        public Query(IMetadataStore metadatastore,
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect, IEnvironmentSettings environment)
        {
            this._metadatastore = metadatastore;
            this._hydrator = hydrator;
            this._connection = connection;
            _dialect = dialect;
            _environment = environment;
            _selectionOptions = new List<ISelectOption>();
            _groupingOptions = new List<IGroupByOption>();
            _orderOptions = new List<IOrderOption>();
            _criteriaRestrictions = new List<ICriteriaRestriction>();
            _queryParameters = new HashSet<QueryParameter>();
            _eagerFetchProperties = new Dictionary<Type, string>();
            _parentEntity = typeof(TParentEntity);
        }

        public string CurrentStatement
        {
            get { return _currentStatement; }
        }

        public IQuery<TParentEntity> JoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                          Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var innerJoinAction = new InnerJoinAction<TParentEntity, TChildEntity>(_metadatastore);
            innerJoinAction.Enqueue(parent, child);
            _joinActions.Add(innerJoinAction);
            return this;
        }

        public IQuery<TParentEntity> OuterJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                               Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var outerJoinAction = new OuterJoinAction<TParentEntity, TChildEntity>(_metadatastore);
            outerJoinAction.Enqueue(parent, child);
            _joinActions.Add(outerJoinAction);
            return this;
        }

        public IQuery<TParentEntity> LeftJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                              Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var leftJoinAction = new LeftJoinAction<TParentEntity, TChildEntity>(_metadatastore);
            leftJoinAction.Enqueue(parent, child);
            _joinActions.Add(leftJoinAction);
            return this;
        }

        public IQuery<TParentEntity> CreateCriteria(params ICriteriaRestriction[] restrictions)
        {
            foreach ( ICriteriaRestriction restriction in restrictions )
            {
                var andCriteriaRestriction = new AndCriteriaSelection();
                ICriteriaRestriction currentRestriction;

                if ( typeof(OrCriteriaSelection).IsAssignableFrom(restriction.GetType()) == false )
                {
                    andCriteriaRestriction.AddRestriction(restriction);
                    currentRestriction = andCriteriaRestriction;
                }
                else
                {
                    currentRestriction = restriction;
                }

                if ( _criteriaRestrictions.Contains(currentRestriction) )
                    continue;
                _criteriaRestrictions.Add(currentRestriction);
            }

            return this;
        }

        public IQuery<TParentEntity> EagerFetch(Expression<Func<TParentEntity, IEnumerable>> eagerFetch)
        {
            // TODO: collect properties that should be eagerly loaded and pass them to the executing action:
            throw new NotImplementedException();
        }

        public IQuery<TParentEntity> Select(params ISelectOption[] selections)
        {
            foreach ( ISelectOption selectOption in selections )
            {
                if ( _selectionOptions.Contains(selectOption) )
                    continue;

                _selectionOptions.Add(selectOption);
            }

            return this;
        }

        public IQuery<TParentEntity> AddOrder<TEntity>(IOrderOption<TEntity> order)
        {
            if ( _orderOptions.Contains(order) )
                return this;
            _metadatastore.AddEntity(typeof(TEntity));
            _orderOptions.Add(order);
            return this;
        }

        public IQuery<TParentEntity> GroupBy(params IGroupByOption[] selections)
        {
            foreach ( IGroupByOption selectOption in selections )
            {
                if ( _groupingOptions.Contains(selectOption) )
                    continue;

                _groupingOptions.Add(selectOption);
            }

            return this;
        }

        public IEnumerable<TParentEntity> ToList(int maxResults = 100)
        {
            BuildSql(maxResults);
            _metadatastore.AddEntity(typeof(TParentEntity));

            var listingAction = new ToListAction<TParentEntity>(_metadatastore, _hydrator, 
                _connection, _dialect, _environment);

            return listingAction.GetListing(_currentStatement, _queryParameters);
        }

        public IEnumerable<TEntity> ToList<TEntity>(int maxResults = 100) where TEntity : class
        {
            if ( typeof(TEntity) == typeof(TParentEntity) )
                throw new InvalidOperationException("Can not project " + typeof(TParentEntity).Name + " onto " +
                                                    typeof(TEntity).Name);
            _metadatastore.AddEntity(typeof(TEntity));

            var listingAction = new ToListAction<TEntity>(_metadatastore, _hydrator,
                _connection, _dialect, _environment);

            return listingAction.GetListing(_currentStatement, _queryParameters);
        }

        public TParentEntity SingleOrDefault()
        {
            _metadatastore.AddEntity(typeof(TParentEntity));

            BuildSql(0);

            var uniqueResultAction = new UniqueResultAction<TParentEntity>(_metadatastore, _hydrator, 
                _connection, _dialect, _environment);

            return uniqueResultAction.GetSingleOrDefaultResult(_currentStatement, _queryParameters);
        }

        public TEntity SingleOrDefault<TEntity>() where TEntity : class
        {
            if ( typeof(TEntity) == typeof(TParentEntity) )
                throw new InvalidOperationException("Can not project " + typeof(TParentEntity).Name + " onto " +
                                                    typeof(TEntity).Name +
                                                    ". The projection entity type can not be the same as the entity to be queried.");

            _metadatastore.AddEntity(typeof(TEntity));

            BuildSql(0);

            var uniqueResultAction = new UniqueResultAction<TEntity>(_metadatastore, 
                _hydrator, _connection, _dialect, _environment);

            return uniqueResultAction.GetSingleOrDefaultResult(_currentStatement, _queryParameters);
        }

        private void BuildSql(int rowsToReturn = 0)
        {
            var builder = new StringBuilder();

            BuildSelect(builder, rowsToReturn);
            BuildJoins(builder);
            BuildRestrictions(builder);
            BuildGroupBy(builder);
            BuildOrderBy(builder);
            _currentStatement = builder.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
        }

        private void BuildSelect(StringBuilder builder, int rowsToReturn = 0)
        {
            TableInfo tableinfo = _metadatastore.GetTableInfo<TParentEntity>();
            string tableName = tableinfo.TableName;

            var fields = new List<string>();

            string selectedFields = string.Empty;
            string separator = ", ";

            if ( _selectionOptions.Count == 0 )
                throw new InvalidOperationException(
                    "In order to return results from a query, a selection option must be indicated for the fields desired. " +
                    "Please use the Select(SelectionOptions.(...)) option on the query object to choose the desired fields for the query.");

            foreach ( ISelectOption selectionOption in _selectionOptions )
            {
                selectionOption.MetadataStore = _metadatastore;
                selectionOption.Build();

                fields.AddRange(selectionOption.Fields);
            }

            foreach ( string field in fields )
            {
                selectedFields = string.Concat(selectedFields, field, separator);
            }

            selectedFields = selectedFields.TrimEnd(separator.ToCharArray());

            string rows = string.Empty;

            if ( rowsToReturn > 0 )
                rows = string.Format("top {0}", rowsToReturn);

            var selectBuilder = new StringBuilder("select");

            if ( string.IsNullOrEmpty(rows.Trim()) == false )
            {
                selectBuilder.AppendFormat(" {0}", rows.Trim());
            }

            if ( string.IsNullOrEmpty(selectedFields.Trim()) == false )
            {
                selectBuilder.AppendFormat(" {0}", selectedFields.Trim());
            }

            selectBuilder.AppendFormat("{0} from [{1}] (nolock)", System.Environment.NewLine, tableName.Trim());

            string select = string.Format("select {0} {1} {2}from [{3}] (nolock)",
                                          string.Format("{0}", rows.Trim()),
                                          selectedFields.Trim(),
                                          System.Environment.NewLine,
                                          tableName.Trim());

            builder.AppendLine(selectBuilder.ToString());
        }

        private void BuildRestrictions(StringBuilder builder)
        {
            var conditions = new List<string>();

            foreach ( ICriteriaRestriction criteriaRestriction in _criteriaRestrictions )
            {
                criteriaRestriction.MetadataStore = _metadatastore;
                criteriaRestriction.Build();

                if ( typeof(AndCriteriaSelection).IsAssignableFrom(criteriaRestriction.GetType()) )
                {
                    var andCriteria = criteriaRestriction as AndCriteriaSelection;
                    new List<QueryParameter>(andCriteria.GetParameters()).ForEach(p => _queryParameters.Add(p));
                }
                else if ( typeof(OrCriteriaSelection).IsAssignableFrom(criteriaRestriction.GetType()) )
                {
                    var orCriteria = criteriaRestriction as OrCriteriaSelection;
                    new List<QueryParameter>(orCriteria.GetParameters()).ForEach(p => _queryParameters.Add(p));
                }

                conditions.AddRange(criteriaRestriction.Expressions);
            }

            if ( conditions.Count > 0 )
            {
                builder.Append(" where ");

                if ( conditions.Count == 1 )
                {
                    if ( conditions[0].Trim().StartsWith("or") )
                    {
                        builder.Append(conditions[0].Substring(2));
                    }
                    else if ( conditions[0].Trim().StartsWith("and") )
                    {
                        builder.Append(conditions[0].Substring(3));
                    }
                }
                else
                {
                    builder.AppendLine();
                    foreach ( string condition in conditions )
                    {
                        builder.AppendLine(condition);
                    }
                }
            }
        }

        private void BuildJoins(StringBuilder builder)
        {
            _joinActions.ForEach(ja => ja.Build(builder));
        }

        private void BuildGroupBy(StringBuilder builder)
        {
            var fields = new List<string>();

            string selectedFields = string.Empty;
            string separator = ", ";

            foreach ( IGroupByOption groupingOption in _groupingOptions )
            {
                groupingOption.MetadataStore = _metadatastore;
                groupingOption.Build();

                fields.AddRange(groupingOption.Fields);
            }

            foreach ( string field in fields )
            {
                selectedFields = string.Concat(selectedFields, field, separator);
            }

            selectedFields = selectedFields.TrimEnd(separator.ToCharArray());

            if ( fields.Count > 0 )
            {
                string grouping = string.Format("group by {0}", selectedFields);
                builder.AppendLine(grouping);
            }
        }

        private void BuildOrderBy(StringBuilder builder)
        {
            var fields = new List<string>();
            string selectedFields = string.Empty;
            string separator = ", ";

            foreach ( IOrderOption orderOption in _orderOptions )
            {
                orderOption.MetadataStore = _metadatastore;
                orderOption.Build();
                fields.AddRange(orderOption.Fields);
            }

            foreach ( string field in fields )
            {
                selectedFields = string.Concat(selectedFields, field, separator);
            }

            selectedFields = selectedFields.TrimEnd(separator.ToCharArray());

            if ( string.IsNullOrEmpty(selectedFields) == false )
                builder.Append("order by ").AppendLine(selectedFields);
        }
    }
}