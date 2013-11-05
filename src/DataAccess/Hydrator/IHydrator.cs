using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Hydrator
{
	public interface IHydrator
	{
		IMetadataStore MetadataStore { get; }
		TEntity HydrateEntity<TEntity>(SqlCommand command) where TEntity : class;
		IEnumerable<TEntity> HydrateEntities<TEntity>(SqlCommand command) where TEntity : class;
		void UpdateEntity(Type targetPropertyType, Type targetToUpdate, object entityInstance, SqlCommand command);
		void InsertEntity<TEntity>(TEntity entity, SqlCommand command);
	}
}