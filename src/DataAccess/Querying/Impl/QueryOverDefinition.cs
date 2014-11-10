using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MicroORM.Configuration;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Joins;
using MicroORM.DataAccess.Querying.Joins.Impl;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
    internal class QueryOverDefinition<TParent> where TParent : class 
    {
        private readonly Type _queryOverEntity;
        private readonly IDbConnection _connection;
        private readonly IMetadataStore _metadatastore;
        private readonly IHydrator _hydrator;
        private readonly IDialect _dialect;
        private readonly IEnvironmentSettings _environment;
        private readonly List<Type> _referencedEntities = new List<Type>();
        private readonly List<IJoinAction> _joins = new List<IJoinAction>();
        private QueryClause _clause;

        public QueryOverDefinition(Type queryOverEntity,
            IDbConnection connection,
            IMetadataStore metadatastore,
            IHydrator hydrator,
            IDialect dialect,
            IEnvironmentSettings environment)
        {
            _queryOverEntity = queryOverEntity;
            _connection = connection;
            _metadatastore = metadatastore;
            _hydrator = hydrator;
            _dialect = dialect;
            _environment = environment;
        }

        public void AddJoin<TLeft, TRight>(InnerJoinAction<TLeft, TRight> action)
            where TLeft : class, new()
            where TRight : class, new()
        {
            TryAddEntity(typeof(TLeft));
            TryAddEntity(typeof(TRight));

            if ( _joins.Contains(action) == false )
                _joins.Add(action);
        }

        public void AddJoin<TLeft, TRight>(LeftJoinAction<TLeft, TRight> action)
            where TLeft : class, new()
            where TRight : class, new()
        {
            TryAddEntity(typeof(TLeft));
            TryAddEntity(typeof(TRight));

            if ( _joins.Contains(action) == false )
                _joins.Add(action);
        }

        public void AddJoin<TLeft, TRight>(RightJoinAction<TLeft, TRight> action)
            where TLeft : class, new()
            where TRight : class, new()
        {
            TryAddEntity(typeof(TLeft));
            TryAddEntity(typeof(TRight));

            if ( _joins.Contains(action) == false )
                _joins.Add(action);
        }

        public IQueryClause<TParent, TOther> AddClause<TOther>(Expression<Func<TOther, bool>> criteria) where TOther : class
        {
            TryAddEntity(typeof(TOther));
            _clause = new QueryClause<TParent, TOther>(this, _queryOverEntity, _connection, _metadatastore, _hydrator, _dialect, _environment, criteria);
            return _clause as IQueryClause<TParent, TOther>;
        }

        public void AddClause<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> criteria)
        {
            TryAddEntity(typeof(TLeft));
            TryAddEntity(typeof(TRight));
        }

        public void Parse(out string statement, out IDictionary<string, object> parameters)
        {
            statement = string.Empty;
            parameters = new Dictionary<string, object>();

            var query = "select {0} from [{1}] {2} where {3}";

            var topViewTableInfo =
                _metadatastore.GetTableInfo(_queryOverEntity);

            var columns = string.Join(",",
                topViewTableInfo.GetFieldsForSelect().Select(field =>
                    string.Format("[{0}].[{1}]",
                        topViewTableInfo.TableName, field)));

            var joins = ProcessAllJoins();
            var criteria = ProcessAllClauses(out parameters);

            statement = string.Format(query, columns, topViewTableInfo.TableName, joins, criteria);
        }

        private string ProcessAllJoins()
        {
            var result = string.Empty;
            var builder = new StringBuilder();

            foreach ( var join in this._joins )
            {
                join.Build(builder);
                builder.Append(string.Empty);
            }

            result = builder.ToString();

            return result;
        }

        private string ProcessAllClauses(out IDictionary<string, object> parameters )
        {
            var builder = new StringBuilder();
            var mergedParameters = new Dictionary<string, object>();

            parameters = new Dictionary<string, object>();

            _clause.Parse();
            MergeParameters(_clause.Parameters, mergedParameters);
            builder.AppendLine(_clause.Criteria);

            _clause.SubClauses.ForEach(sc =>
            {
                sc.Parse();
                builder.AppendLine(sc.Criteria);
                MergeParameters(sc.Parameters, mergedParameters);
            });

            parameters = mergedParameters;
            return builder.ToString();
        }

        private void MergeParameters(IDictionary<string, object> source, IDictionary<string, object> destination)
        {
            foreach (var kvp in source)
            {
                if (destination.ContainsKey(kvp.Key) == false)
                {
                    destination.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private void TryAddEntity(Type entity)
        {
            _metadatastore.AddEntity(entity);
            TryAddReferencedEntity(entity);
        }

        private void TryAddReferencedEntity(Type entity)
        {
            if ( _referencedEntities.Any(e => e == entity) )
                return;
            _referencedEntities.Add(entity);
        }
    }
}