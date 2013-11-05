using System;

namespace MicroORM.DataAccess.Internals
{
	public interface ISessionCache
	{
		void Save<TEntity>(TEntity entity, object id) where TEntity : class;
		void Remove<TEntity>(TEntity entity, object id) where TEntity : class;
		void Clear<TEntity>() where TEntity : class;
		void Clear(Type entity);
		void ClearAll();
		bool TryFind<TEntity>(object id, out TEntity entity) where TEntity : class;
	}
}