using System;
using System.Collections.Generic;

namespace MicroORM.DataAccess.Internals.Impl
{
	internal class SessionCache : ISessionCache
	{
		private readonly IDictionary<Type, IDictionary<object, object>> cache;

		public SessionCache()
		{
			this.cache = new Dictionary<Type, IDictionary<object, object>>();
		}

		public bool TryFind<TEntity>(object id, out TEntity entity) where TEntity : class
		{
			bool success = false;
			entity = default(TEntity);

			if (this.cache.ContainsKey(typeof (TEntity)))
			{
				var references = this.cache[typeof (TEntity)];

				if (references.ContainsKey(id) == true)
				{
					entity = references[id] as TEntity;
					success = entity != null ? true : false;
				}
			}

			return success;
		}

		public void Save<TEntity>(TEntity entity, object id) where TEntity : class
		{
			if (entity == null) return;

			if (this.cache.ContainsKey(typeof (TEntity)) == false)
			{
				// create the identity map for the instances:
				var references = new Dictionary<object, object>();
				references.Add(id, entity);
				this.cache[typeof (TEntity)] = references;
			}
			else
			{
				// enqueue the instance into the identity map:
				var references = this.cache[typeof (TEntity)];

				if (references.ContainsKey(id) == false)
				{
					// insert:
					references.Add(id, entity);
				}
				else
				{
					// update:
					references[id] = entity;
				}
			}
		}

		public void Remove<TEntity>(TEntity entity, object id) where TEntity : class
		{
			if (this.cache.ContainsKey(typeof (TEntity)))
			{
				var references = this.cache[typeof (TEntity)];
				if (references.ContainsKey(id))
				{
					references.Remove(id);
				}
			}
		}

		public void Clear<TEntity>() where TEntity : class
		{
			this.Clear(typeof (TEntity));
		}

		public void Clear(Type entity)
		{
			this.cache.Remove(entity);
		}

		public void ClearAll()
		{
			this.cache.Clear();
		}
	}
}