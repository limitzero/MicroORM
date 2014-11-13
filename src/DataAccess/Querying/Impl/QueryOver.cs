using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Joins.Impl;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
    public class QueryOver<TEntity> : IQueryOver<TEntity> where TEntity : class, new()
    {
        private readonly IMetadataStore _metadatastore;
        private readonly IHydrator _hydrator;
        private readonly IDbConnection _connection;
        private readonly IDialect _dialect;
        private readonly IEnvironmentSettings _environment;
        private readonly QueryOverDefinition<TEntity> _queryDefinition;

        public QueryOver(IMetadataStore metadatastore,
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect, IEnvironmentSettings environment)
        {
            _metadatastore = metadatastore;
            _hydrator = hydrator;
            _connection = connection;
            _dialect = dialect;
            _environment = environment;

            _queryDefinition = new QueryOverDefinition<TEntity>(typeof(TEntity),
                _connection, _metadatastore, _hydrator, _dialect, _environment);
        }

        public IQueryOver<TEntity> Join<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new()
        {
            var action = new InnerJoinAction<TEntity, TOther>(_metadatastore);
            action.Enqueue(entity, other);
            _queryDefinition.AddJoin(action);
            return this;
        }

        public IQueryOver<TEntity> LeftJoin<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new()
        {
            var action = new LeftJoinAction<TEntity, TOther>(_metadatastore);
            action.Enqueue(entity, other);
            _queryDefinition.AddJoin(action);
            return this;
        }

        public IQueryOver<TEntity> RightJoin<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new()
        {
            var action = new RightJoinAction<TEntity, TOther>(_metadatastore);
            action.Enqueue(entity, other);
            _queryDefinition.AddJoin(action);
            return this;
        }

        public IQueryClause<TEntity, TOther> Where<TOther>(Expression<Func<TOther, bool>> criteria)
            where TOther : class, new()
        {
            var clause = _queryDefinition.AddClause<TOther>(criteria);
            return clause;
        }

        public IQueryClause<TEntity, TEntity> Where(Expression<Func<TEntity, bool>> criteria)
        {
            var clause = _queryDefinition.AddClause(criteria);
            return clause;
        }

        public IEnumerable<TEntity> Select()
        {
            var statement = string.Empty;
            IDictionary<string, object> parameters = null;

            _queryDefinition.Parse(out statement, out parameters);

            var listingAction = new ToListAction<TEntity>(_metadatastore,
            this._hydrator, this._connection, _dialect, _environment);

            var entities = listingAction.GetListing(statement, parameters);

            return entities;
        }

        public IEnumerable<TProjection> Select<TProjection>() where TProjection : class, new()
        {
            _metadatastore.AddEntity(typeof(TProjection));

            var statement = string.Empty;
            IDictionary<string, object> parameters = null;

            _queryDefinition.Parse(out statement, out parameters);

            var listingAction = new ToListAction<TProjection>(_metadatastore,
                this._hydrator, this._connection, _dialect, _environment);

            var projections = listingAction.GetListing(statement, parameters);

            return projections;
        }
    }
}