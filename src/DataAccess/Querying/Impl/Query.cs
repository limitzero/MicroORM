using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
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
        private readonly IDbConnection connection;
        private readonly IDialect _dialect;
        private readonly List<ICriteriaRestriction> criteriaRestrictions;
        private readonly IDictionary<Type, string> eagerFetchProperties;
        private readonly List<IGroupByOption> groupingOptions;
        private readonly IHydrator hydrator;
        private readonly List<IJoinAction> joinActions = new List<IJoinAction>();
        private readonly IMetadataStore metadatastore;
        private readonly List<IOrderOption> orderOptions;
        private readonly Type parentEntity;
        private readonly HashSet<QueryParameter> queryParameters;
        private readonly List<ISelectOption> selectionOptions;
        private string currentStatement;

        public Query(IMetadataStore metadatastore,
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect)
        {
            this.metadatastore = metadatastore;
            this.hydrator = hydrator;
            this.connection = connection;
            _dialect = dialect;
            selectionOptions = new List<ISelectOption>();
            groupingOptions = new List<IGroupByOption>();
            orderOptions = new List<IOrderOption>();
            criteriaRestrictions = new List<ICriteriaRestriction>();
            queryParameters = new HashSet<QueryParameter>();
            eagerFetchProperties = new Dictionary<Type, string>();
            parentEntity = typeof(TParentEntity);
        }

        public string CurrentStatement
        {
            get { return currentStatement; }
        }

        public IQuery<TParentEntity> JoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                          Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var innerJoinAction = new InnerJoinAction<TParentEntity, TChildEntity>(metadatastore);
            innerJoinAction.Enqueue(parent, child);
            joinActions.Add(innerJoinAction);
            return this;
        }

        public IQuery<TParentEntity> OuterJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                               Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var outerJoinAction = new OuterJoinAction<TParentEntity, TChildEntity>(metadatastore);
            outerJoinAction.Enqueue(parent, child);
            joinActions.Add(outerJoinAction);
            return this;
        }

        public IQuery<TParentEntity> LeftJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
                                                              Expression<Func<TParentEntity, object>> parent)
            where TChildEntity : class, new()
        {
            var leftJoinAction = new LeftJoinAction<TParentEntity, TChildEntity>(metadatastore);
            leftJoinAction.Enqueue(parent, child);
            joinActions.Add(leftJoinAction);
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

                if ( criteriaRestrictions.Contains(currentRestriction) )
                    continue;
                criteriaRestrictions.Add(currentRestriction);
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
                if ( selectionOptions.Contains(selectOption) )
                    continue;

                selectionOptions.Add(selectOption);
            }

            return this;
        }

        public IQuery<TParentEntity> AddOrder<TEntity>(IOrderOption<TEntity> order)
        {
            if ( orderOptions.Contains(order) )
                return this;
            metadatastore.AddEntity(typeof(TEntity));
            orderOptions.Add(order);
            return this;
        }

        public IQuery<TParentEntity> GroupBy(params IGroupByOption[] selections)
        {
            foreach ( IGroupByOption selectOption in selections )
            {
                if ( groupingOptions.Contains(selectOption) )
                    continue;

                groupingOptions.Add(selectOption);
            }

            return this;
        }

        public IEnumerable<TParentEntity> ToList(int maxResults = 100)
        {
            BuildSql(maxResults);
            metadatastore.AddEntity(typeof(TParentEntity));

            var listingAction = new ToListAction<TParentEntity>(metadatastore, hydrator, connection, _dialect);
            return listingAction.GetListing(currentStatement, queryParameters);
        }

        public IEnumerable<TEntity> ToList<TEntity>(int maxResults = 100) where TEntity : class
        {
            if ( typeof(TEntity) == typeof(TParentEntity) )
                throw new InvalidOperationException("Can not project " + typeof(TParentEntity).Name + " onto " +
                                                    typeof(TEntity).Name);
            metadatastore.AddEntity(typeof(TEntity));

            var listingAction = new ToListAction<TEntity>(metadatastore, hydrator, connection, _dialect);
            return listingAction.GetListing(currentStatement, queryParameters);
        }

        public TParentEntity SingleOrDefault()
        {
            metadatastore.AddEntity(typeof(TParentEntity));

            BuildSql(0);

            var uniqueResultAction = new UniqueResultAction<TParentEntity>(metadatastore, hydrator, connection, _dialect);
            return uniqueResultAction.GetSingleOrDefaultResult(currentStatement, queryParameters);
        }

        public TEntity SingleOrDefault<TEntity>() where TEntity : class
        {
            if ( typeof(TEntity) == typeof(TParentEntity) )
                throw new InvalidOperationException("Can not project " + typeof(TParentEntity).Name + " onto " +
                                                    typeof(TEntity).Name +
                                                    ". The projection entity type can not be the same as the entity to be queried.");

            metadatastore.AddEntity(typeof(TEntity));

            BuildSql(0);

            var uniqueResultAction = new UniqueResultAction<TEntity>(metadatastore, hydrator, connection, _dialect);
            return uniqueResultAction.GetSingleOrDefaultResult(currentStatement, queryParameters);
        }

        private void BuildSql(int rowsToReturn = 0)
        {
            var builder = new StringBuilder();

            BuildSelect(builder, rowsToReturn);
            BuildJoins(builder);
            BuildRestrictions(builder);
            BuildGroupBy(builder);
            BuildOrderBy(builder);
            currentStatement = builder.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
        }

        private void BuildSelect(StringBuilder builder, int rowsToReturn = 0)
        {
            TableInfo tableinfo = metadatastore.GetTableInfo<TParentEntity>();
            string tableName = tableinfo.TableName;

            var fields = new List<string>();

            string selectedFields = string.Empty;
            string separator = ", ";

            if ( selectionOptions.Count == 0 )
                throw new InvalidOperationException(
                    "In order to return results from a query, a selection option must be indicated for the fields desired. " +
                    "Please use the Select(SelectionOptions.(...)) option on the query object to choose the desired fields for the query.");

            foreach ( ISelectOption selectionOption in selectionOptions )
            {
                selectionOption.MetadataStore = metadatastore;
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

            foreach ( ICriteriaRestriction criteriaRestriction in criteriaRestrictions )
            {
                criteriaRestriction.MetadataStore = metadatastore;
                criteriaRestriction.Build();

                if ( typeof(AndCriteriaSelection).IsAssignableFrom(criteriaRestriction.GetType()) )
                {
                    var andCriteria = criteriaRestriction as AndCriteriaSelection;
                    new List<QueryParameter>(andCriteria.GetParameters()).ForEach(p => queryParameters.Add(p));
                }
                else if ( typeof(OrCriteriaSelection).IsAssignableFrom(criteriaRestriction.GetType()) )
                {
                    var orCriteria = criteriaRestriction as OrCriteriaSelection;
                    new List<QueryParameter>(orCriteria.GetParameters()).ForEach(p => queryParameters.Add(p));
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
            joinActions.ForEach(ja => ja.Build(builder));
        }

        private void BuildGroupBy(StringBuilder builder)
        {
            var fields = new List<string>();

            string selectedFields = string.Empty;
            string separator = ", ";

            foreach ( IGroupByOption groupingOption in groupingOptions )
            {
                groupingOption.MetadataStore = metadatastore;
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

            foreach ( IOrderOption orderOption in orderOptions )
            {
                orderOption.MetadataStore = metadatastore;
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