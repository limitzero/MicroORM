using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using LinqExtender;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Hydrator.Impl;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.DataAccess.Querying;
using MicroORM.DataAccess.Querying.Impl;
using MicroORM.Interception;
using MicroORM.Interception.Impl;

namespace MicroORM.DataAccess.Internals.Impl
{
	// TODO: not thread-safe...will need to work on this
	internal class Session : ISession
	{
		private bool disposed = false;
		private SqlConnection connection;
		private IMetadataStore metadataStore;
		private IHydrator hydrator;
		private ISessionCache sessionCache;
		private IInterceptorPipeline interceptorPipeline;
		private ITransaction transaction;

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

		public Session(SqlConnection connection, IMetadataStore metadataStore)
			: this()
		{
			this.connection = connection;
			this.metadataStore = metadataStore;
			this.InitializeSession(this.metadataStore, this.connection);
		}

		public Session()
		{
			this.InitializeSession(this.metadataStore, this.connection);
		}

		public void Dispose()
		{
			this.Disposing(true);
			GC.SuppressFinalize(this);
		}

		public TEntity Get<TEntity>(object id) where TEntity : class
		{
			TEntity entity = default(TEntity);

			if (this.disposed) return entity;

			this.metadataStore.AddEntity(typeof (TEntity));

			var getByIdAction = new GetByIdAction<TEntity>(this.metadataStore,
			                                               null, this.hydrator, this.connection);

			entity = getByIdAction.GetById(id);

			this.sessionCache.Save<TEntity>(entity, id);

			return entity;
		}

		public TEntity Load<TEntity>(object id) where TEntity : class
		{
			TEntity entity = default(TEntity);

			if (this.disposed) return entity;

			if (this.sessionCache.TryFind<TEntity>(id, out entity) == false)
				throw new Exception(string.Format("The entity '{0}'can not be loaded in the session without " +
				                                  "first retrieving it by its primary key.",
				                                  typeof (TEntity).FullName));

#if DEBUG
			Type theEntity = entity.GetType().IsProxy() ? entity.GetType().BaseType : entity.GetType();
			System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Loaded hydrated entity '{0}' " +
			                                                 "from session cache with primay key identifier '{1}'.",
			                                                 theEntity.FullName,
			                                                 id.ToString()));
#endif

			return entity;
		}

		public void Save<TEntity>(TEntity entity) where TEntity : class
		{
			this.SaveOrUpdate(entity);
		}

		public void SaveOrUpdate<TEntity>(TEntity entity) where TEntity : class
		{
			if (this.disposed || entity == null) return;

			if (this.transaction != null)
			{
				((Transaction) this.transaction).Enqueue(() => this.SaveOrUpdateInternal(entity));
			}
			else
			{
				SaveOrUpdateInternal(entity);
			}
		}

		private void SaveOrUpdateInternal<TEntity>(TEntity entity) where TEntity : class
		{
			this.metadataStore.AddEntity(typeof (TEntity));
			var tableInfo = this.metadataStore.GetTableInfo<TEntity>();

			DisableLazyLoading(entity);

			UpsertEntity(tableInfo, entity);

			// store the parent entity in the session cache, lazy loading will handle the rest:
			this.sessionCache.Save(entity, tableInfo.GetPrimaryKeyValue(entity));

			UpsertEntityCollections(tableInfo, entity);

			EnableLazyLoading(entity);
		}

		public void Delete<TEntity>(TEntity entity) where TEntity : class
		{
			if (this.disposed || entity == null) return;

			if (this.transaction != null)
			{
				((Transaction) this.transaction).Enqueue(() => this.DeleteIntenal(entity));
			}
			else
			{
				DeleteIntenal(entity);
			}
		}

		private void DeleteIntenal<TEntity>(TEntity entity) where TEntity : class
		{
			this.metadataStore.AddEntity(typeof (TEntity));
			var tableInfo = this.metadataStore.GetTableInfo<TEntity>();

			var deleteAction = new DeleteAction<TEntity>(this.metadataStore, entity, this.connection);
			Action proceed = () => deleteAction.Delete(entity);

			var invocation = new DataInvocation(this, this.metadataStore,
			                                    entity, proceed);

			interceptorPipeline.ExecuteOnDelete(invocation);

			this.sessionCache.Remove(entity, tableInfo.GetPrimaryKeyValue(entity));
		}

		public IQuery<T> CreateQueryFor<T>() where T : class, new()
		{
			this.metadataStore.AddEntity(typeof (T));
			return new Querying.Impl.Query<T>(this.metadataStore, this.hydrator, this.connection);
		}

		public IQueryContext<T> QueryOver<T>() where T : class, new()
		{
			this.metadataStore.AddEntity(typeof (T));
			return new QueryContext<T>(this.metadataStore, this.hydrator, this.connection);
		}

		public IQueryByStoredProcedure ExecuteProcedure()
		{
			return new QueryByStoredProcedure(this.metadataStore, this.hydrator, this.connection);
		}

		public ITransaction BeginTransaction()
		{
			this.transaction = new Transaction(this);
			return this.transaction;
		}

		public void InitializeProxy(string targetPropertyName, object entity, Type targetType)
		{
			if (this.disposed || entity == null) return;
#if DEBUG
			Type theEntity = entity.GetType().IsProxy() ? entity.GetType().BaseType : entity.GetType();
			System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Lazy loading property '{0}' " +
			                                                 "with property type '{1}' on entity '{2}'.",
			                                                 targetPropertyName,
			                                                 targetType.FullName,
			                                                 theEntity.FullName));
#endif
			var intializeProxyAction = new InitializeProxyAction(this.metadataStore, this.hydrator, this.connection);
			intializeProxyAction.InitializeProxy(entity, targetPropertyName, targetType);
		}

		private void InitializeSession(IMetadataStore metadataStore, SqlConnection connection)
		{
			if (metadataStore != null)
			{
				this.metadataStore = metadataStore;
			}
			else
			{
				this.metadataStore = new MetadataStore();
			}

			if (this.connection != null)
			{
				if (this.connection.State != ConnectionState.Open)
				{
					this.connection.Open();
				}
			}

			this.hydrator = new EntityHydrator(this.metadataStore, this);
			this.sessionCache = new SessionCache();
			this.interceptorPipeline = new InterceptorPipeline();
		}

		private static void DisableLazyLoading<TEntity>(TEntity entity)
		{
			if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
			{
				((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = false;
			}
		}

		private static void EnableLazyLoading<TEntity>(TEntity entity)
		{
			if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
			{
				((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = true;
			}
		}

		private void UpsertEntity<TEntity>(TableInfo tableInfo, TEntity entity)
			where TEntity : class
		{
			if (tableInfo.IsPrimaryKeySet(entity))
			{
				// update:
				var updateAction = new UpdateAction<TEntity>(this.metadataStore,
				                                             entity, this.hydrator, this.connection);
				Action proceed = () => updateAction.Update(entity);

				var invocation = new DataInvocation(this, this.metadataStore,
				                                    entity, proceed);

				interceptorPipeline.ExecuteOnUpdate(invocation);
			}
			else
			{
				// insert:
				var insertAction = new InsertAction<TEntity>(this.metadataStore,
				                                             entity, this.hydrator, this.connection);
				Action proceed = () => insertAction.Insert(entity);


				var invocation = new DataInvocation(this, this.metadataStore,
				                                    entity, proceed);

				interceptorPipeline.ExecuteOnInsert(invocation);
			}
		}

		private void UpsertEntityReferences<TEntity>(TableInfo tableInfo, TEntity entity)
			where TEntity : class
		{
			var referenceColumns = tableInfo.References;

			foreach (var referenceColumn in referenceColumns)
			{
				var property = entity.GetType().GetProperty(referenceColumn.Column.Name);
				var contents = property.GetValue(entity, null);

				if (contents != null)
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

			if (collectionColumns.Count > 0)
			{
				foreach (var collectionColumn in collectionColumns)
				{
					var property = entity.GetType().GetProperty(collectionColumn.Column.Name);
					var collection = property.GetValue(entity, null) as IEnumerable;

					if (collection != null)
					{
						var iterator = collection.GetEnumerator();

						while (iterator.MoveNext())
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
			if (this.disposed) return;

			var method = this.GetType().GetMethod("SaveOrUpdate");
			var genericMethod = method.MakeGenericMethod(new Type[] {entity.GetType()});
			genericMethod.Invoke(this, new object[] {entity});
		}

		private void Disposing(bool isDisposing)
		{
			if (isDisposing)
			{
				this.disposed = true;

				if (this.connection != null)
				{
					this.connection.Close();
				}
				this.connection = null;

				if (this.sessionCache != null)
				{
					this.sessionCache.ClearAll();
				}
				this.sessionCache = null;
			}
		}

		private static void CreateConnectionFromAlias(string alias)
		{
			// get from connection settings in config file:
		}

		private static void SetParentEntityOnChildEntity(object parent, object child)
		{
			var property = (from match in child.GetType().GetProperties()
			                where match.PropertyType == parent.GetType()
			                select match).FirstOrDefault();

			if (property != null)
			{
				property.SetValue(child, parent, null);
			}
		}
	}
}