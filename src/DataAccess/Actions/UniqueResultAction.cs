using System.Collections.Generic;
using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Actions
{
	public class UniqueResultAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public UniqueResultAction(IMetadataStore metadataStore, IHydrator hydrator, SqlConnection connection) :
			base(metadataStore, default(TEntity), connection)
		{
			this.hydrator = hydrator;
		}

		public TEntity GetSingleOrDefaultResult(string statement, ICollection<QueryParameter> parameters)
		{
			TEntity entity = default(TEntity);

			using (var command = this.CreateCommand())
			{
				command.CommandText = statement;
				command.CreateParametersFromQuery(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entity = hydrator.HydrateEntity<TEntity>(command);

					// force lazy loading on hydrated entity (if possible):
					if (entity != null)
					{
						if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
						{
							((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = true;
						}
					}
				}
			}

			return entity;
		}

		public TEntity GetSingleOrDefaultResult(string statement, IDictionary<string, object> parameters)
		{
			TEntity entity = default(TEntity);

			using (var command = this.CreateCommand())
			{
				command.CommandText = statement;
				command.CreateParametersFromDictionary(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entity = hydrator.HydrateEntity<TEntity>(command);

					// force lazy loading on hydrated entity (if possible):
					if (entity != null)
					{
						if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
						{
							((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = true;
						}
					}
				}
			}

			return entity;
		}
	}
}