using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.DataAccess.Querying.Impl;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class ToListAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public ToListAction(IMetadataStore metadataStore, 
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect) :
			base(metadataStore, default(TEntity), connection, dialect)
		{
			this.hydrator = hydrator;
		}

		public IEnumerable<TEntity> GetListing(string statement, ICollection<QueryParameter> parameters)
		{
			IEnumerable<TEntity> entities = new List<TEntity>();

			using (var command = this.CreateCommand())
			{
				command.CommandText = statement;
				command.CreateParametersFromQuery(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entities = hydrator.HydrateEntities<TEntity>(command);
				}
			}

			return entities;
		}

		public IEnumerable<TEntity> GetListing(string statement, IDictionary<string, object> parameters)
		{
			IEnumerable<TEntity> entities = new List<TEntity>();
			IList<TEntity> lazyEntities = new List<TEntity>();

			using (var command = this.CreateCommand())
			{
				command.CommandText = statement;
				command.CreateParametersFromDictionary(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entities = hydrator.HydrateEntities<TEntity>(command);

					foreach (var entity in entities)
					{
						// force lazy loading on hydrated entity (if possible):
						if (entity != null)
						{
							if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
							{
								((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = true;
							}
						}
						lazyEntities.Add(entity);
					}
				}
			}

			return lazyEntities;
		}
	}
}