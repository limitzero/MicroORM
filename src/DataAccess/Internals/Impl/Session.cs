using System;
using System.Collections;
using System.Data;
using System.Linq;
using LinqExtender;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Hydrator.Impl;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.DataAccess.Querying;
using MicroORM.DataAccess.Querying.Impl;
using MicroORM.Dialects;
using MicroORM.Interception;
using MicroORM.Interception.Impl;

namespace MicroORM.DataAccess.Internals.Impl
{
    // TODO: not thread-safe...will need to work on this
    internal class Session : ISession
    {
        private bool disposed = false;
        private IDbConnection _connection;
        private IMetadataStore _metadataStore;
        private readonly IDialect _dialect;
        private readonly IEnvironmentSettings _environment;
        private IHydrator _hydrator;
        private ISessionCache _sessionCache;
        private IInterceptorPipeline _interceptorPipeline;
        private ITransaction _transaction;

        /// <summary>
        /// Gets or sets the value to determine whether or not to halt the data mutation actions 
        /// for the session based on the current state of the transaction that is managing the changes.
        /// </summary>
        public bool IsTransactionRolledBack { get; set; }

        public Session(string alias)
            : this()
        {
            CreateConnectionFromAlias(alias);
        }

        public Session(IDbConnection connection, IMetadataStore metadataStore,
            IDialect dialect, IEnvironmentSettings environment)
            : this()
        {
            this._connection = connection;
            this._metadataStore = metadataStore;
            _dialect = dialect;
            _environment = environment;
            this.InitializeSession(this._metadataStore, this._connection);
        }

        public Session()
        {
            this.InitializeSession(this._metadataStore, this._connection);
        }

        public void Dispose()
        {
            this.Disposing(true);
            GC.SuppressFinalize(this);
        }

        public TEntity Get<TEntity>(object id) where TEntity : class
        {
            TEntity entity = default(TEntity);

            if ( this.disposed )
                return entity;

            this._metadataStore.AddEntity(typeof(TEntity));

            var getByIdAction = new GetByIdAction<TEntity>(this._metadataStore,
                                                           null, this._hydrator, this._connection,
                                                           this._dialect, this._environment);

            entity = getByIdAction.GetById(id);

            this.InspectForLazyLoading(entity);

            this._sessionCache.Save<TEntity>(entity, id);

            return entity;
        }

        public TEntity Load<TEntity>(object id) where TEntity : class
        {
            TEntity entity = default(TEntity);

            if ( this.disposed )
                return entity;

            if ( this._sessionCache.TryFind<TEntity>(id, out entity) == false )
                throw new Exception(string.Format("The entity '{0}'can not be loaded in the session without " +
                                                  "first retrieving it by its primary key.",
                                                  typeof(TEntity).FullName));

#if DEBUG
            Type theEntity = entity.GetType().IsProxy() ? entity.GetType().BaseType : entity.GetType();
            _environment.Logger.DebugFormat("MicroORM : Loaded hydrated entity '{0}' " +
                                                             "from session cache with primay key identifier '{1}'.",
                                                             theEntity.FullName,
                                                             id.ToString());
#endif

            return entity;
        }

        public void Save<TEntity>(TEntity entity) where TEntity : class
        {
            this.SaveOrUpdate(entity);
        }

        public void SaveOrUpdate<TEntity>(TEntity entity) where TEntity : class
        {
            if ( this.disposed || entity == null )
                return;

            if ( this._transaction != null )
            {
                ( (Transaction)this._transaction ).Enqueue(() => this.SaveOrUpdateInternal(entity));
            }
            else
            {
                SaveOrUpdateInternal(entity);
            }
        }

        private void SaveOrUpdateInternal<TEntity>(TEntity entity) where TEntity : class
        {
            this._metadataStore.AddEntity(typeof(TEntity));
            var tableInfo = this._metadataStore.GetTableInfo<TEntity>();

            DisableLazyLoading(entity);

            UpsertEntity(tableInfo, entity);

            // store the parent entity in the session cache, lazy loading will handle the rest:
            this._sessionCache.Save(entity, tableInfo.GetPrimaryKeyValue(entity));

            UpsertEntityCollections(tableInfo, entity);

            EnableLazyLoading(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if ( this.disposed || entity == null )
                return;

            if ( this._transaction != null )
            {
                ( (Transaction)this._transaction ).Enqueue(() => this.DeleteIntenal(entity));
            }
            else
            {
                DeleteIntenal(entity);
            }
        }

        private void DeleteIntenal<TEntity>(TEntity entity) where TEntity : class
        {
            this._metadataStore.AddEntity(typeof(TEntity));
            var tableInfo = this._metadataStore.GetTableInfo<TEntity>();

            var deleteAction = new DeleteAction<TEntity>(this._metadataStore,
                entity, this._connection, this._dialect, this._environment);

            Action proceed = () => deleteAction.Delete(entity);

            var invocation = new DataInvocation(this, this._metadataStore,
                                                entity, proceed);

            _interceptorPipeline.ExecuteOnDelete(invocation);

            this._sessionCache.Remove(entity, tableInfo.GetPrimaryKeyValue(entity));
        }

        public IQuery<T> CreateQueryFor<T>() where T : class, new()
        {
            this._metadataStore.AddEntity(typeof(T));
            return new Querying.Impl.Query<T>(this._metadataStore, this._hydrator,
                this._connection, _dialect, _environment);
        }

        public IQueryOver<T> QueryOver<T>() where T : class, new()
        {
            var query = new QueryOver<T>(_metadataStore,
                _hydrator, _connection, _dialect, _environment);
            return query;
        }

        [Obsolete]
        private IQueryContext<T> QueryOverImpl<T>() where T : class, new()
        {
            this._metadataStore.AddEntity(typeof(T));
            return new QueryContext<T>(this._metadataStore, this._hydrator,
                this._connection, _dialect, _environment);
        }

        public IQueryByStoredProcedure ExecuteProcedure()
        {
            return new QueryByStoredProcedure(this._metadataStore,
                this._hydrator, this._connection, this._dialect, _environment);
        }

        public ITransaction BeginTransaction()
        {
            this._transaction = new Transaction(this);
            return this._transaction;
        }

        public void InitializeProxy(string targetPropertyName, object entity, Type targetType)
        {
            if ( this.disposed || entity == null )
                return;
#if DEBUG
            Type theEntity = entity.GetType().IsProxy() ? entity.GetType().BaseType : entity.GetType();
            _environment.Logger.DebugFormat("MicroORM : Lazy loading property '{0}' " +
                                                              "with property type '{1}' on entity '{2}'.",
                                                              targetPropertyName,
                                                              targetType.FullName,
                                                              theEntity.FullName);
#endif
            var intializeProxyAction = new InitializeProxyAction(this._metadataStore, this._hydrator,
                this._connection, _environment);
            intializeProxyAction.InitializeProxy(entity, targetPropertyName, targetType);
        }

        private void InitializeSession(IMetadataStore metadataStore, IDbConnection connection)
        {
            if ( metadataStore != null )
            {
                this._metadataStore = metadataStore;
            }
            else
            {
                this._metadataStore = new MetadataStore();
            }

            if ( this._connection != null )
            {
                if ( this._connection.State != ConnectionState.Open )
                {
                    this._connection.Open();
                }
            }

            this._hydrator = new EntityHydrator(this._metadataStore, this);
            this._sessionCache = new SessionCache();
            this._interceptorPipeline = new InterceptorPipeline(_environment);
        }

        private static void DisableLazyLoading<TEntity>(TEntity entity)
        {
            if ( typeof(ILazyLoadSpecification).IsAssignableFrom(entity.GetType()) )
            {
                ( (ILazyLoadSpecification)entity ).IsLazyLoadingEnabled = false;
            }
        }

        private static void EnableLazyLoading<TEntity>(TEntity entity)
        {
            if ( typeof(ILazyLoadSpecification).IsAssignableFrom(entity.GetType()) )
            {
                ( (ILazyLoadSpecification)entity ).IsLazyLoadingEnabled = true;
            }
        }

        private void InspectForLazyLoading<TEntity>(TEntity entity)
        {
            var tableInfo = _metadataStore.GetTableInfo<TEntity>();

            if ( tableInfo.SupportsLazyLoading() )
            {
                EnableLazyLoading(entity);
            }
            else
            {
                TouchAllLazyPropertiesForEagerFetch(entity);
                // DisableLazyLoading(entity);
            }
        }

        private void TouchAllLazyPropertiesForEagerFetch<TEntity>(TEntity entity)
        {
            var tableInfo = _metadataStore.GetTableInfo(entity.GetType());
            var collections = tableInfo.Collections.Where(c => !c.IsLazyLoaded).ToList();
            var references = tableInfo.References.Where(c => !c.IsLazyLoaded).ToList();

            foreach ( var collection in collections )
            {
                var value = collection.Column.GetValue(entity, null) as IEnumerable;
                var iter = value.GetEnumerator();
                while ( iter.MoveNext() )
                {
                    break;
                }
            }

            foreach ( var reference in references )
            {
                reference.Column.GetValue(entity, null);
            }
        }

        private void UpsertEntity<TEntity>(TableInfo tableInfo, TEntity entity)
            where TEntity : class
        {
            if ( tableInfo.IsPrimaryKeySet(entity) )
            {
                // update:
                var updateAction =
                    new UpdateAction<TEntity>(this._metadataStore, entity,
                        this._hydrator, this._connection,
                        this._dialect, this._environment);

                Action proceed = () => updateAction.Update(entity);

                var invocation = new DataInvocation(this, this._metadataStore,
                                                    entity, proceed);

                _interceptorPipeline.ExecuteOnUpdate(invocation);
            }
            else
            {
                // insert:
                var insertAction =
                    new InsertAction<TEntity>(this._metadataStore, entity,
                        this._hydrator, this._connection,
                        this._dialect, this._environment);

                Action proceed = () => insertAction.Insert(entity);


                var invocation = new DataInvocation(this, this._metadataStore,
                                                    entity, proceed);

                _interceptorPipeline.ExecuteOnInsert(invocation);
            }
        }

        private void UpsertEntityReferences<TEntity>(TableInfo tableInfo, TEntity entity)
            where TEntity : class
        {
            var referenceColumns = tableInfo.References;

            foreach ( var referenceColumn in referenceColumns )
            {
                var property = entity.GetType().GetProperty(referenceColumn.Column.Name);
                var contents = property.GetValue(entity, null);

                if ( contents != null )
                {
                    SetParentEntityOnChildEntity(entity, contents);
                    this.SaveOrUpdateInternal(contents);
                }
            }
        }

        private void UpsertEntityCollections<TEntity>(TableInfo tableInfo, TEntity entity)
            where TEntity : class
        {
            var collectionColumns = tableInfo.Collections;

            if ( collectionColumns.Count > 0 )
            {
                foreach ( var collectionColumn in collectionColumns )
                {
                    var property = entity.GetType().GetProperty(collectionColumn.Column.Name);
                    var collection = property.GetValue(entity, null) as IEnumerable;

                    if ( collection != null )
                    {
                        var iterator = collection.GetEnumerator();

                        while ( iterator.MoveNext() )
                        {
                            SetParentEntityOnChildEntity(entity, iterator.Current);
                            this.SaveOrUpdateInternal(iterator.Current);
                        }
                    }
                }
            }
        }

        private void SaveOrUpdateInternal(object entity)
        {
            if ( this.disposed )
                return;

            var method = this.GetType().GetMethod("SaveOrUpdate");
            var genericMethod = method.MakeGenericMethod(new Type[] { entity.GetType() });
            genericMethod.Invoke(this, new object[] { entity });
        }

        private void Disposing(bool isDisposing)
        {
            if ( isDisposing )
            {
                this.disposed = true;

                if ( this._connection != null )
                {
                    this._connection.Close();
                }
                this._connection = null;

                if ( this._sessionCache != null )
                {
                    this._sessionCache.ClearAll();
                }
                this._sessionCache = null;
            }
        }

        private static void CreateConnectionFromAlias(string alias)
        {
            // get from connection settings in config file:
        }

        private static void SetParentEntityOnChildEntity(object parent, object child)
        {
            var property = ( from match in child.GetType().GetProperties()
                             where match.PropertyType == parent.GetType()
                             select match ).FirstOrDefault();

            if ( property != null )
            {
                property.SetValue(child, parent, null);
            }
        }
    }
}