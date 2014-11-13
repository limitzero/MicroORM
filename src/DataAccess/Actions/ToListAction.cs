using System.Collections.Generic;
using System.Data;
using MicroORM.Configuration;
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
		private readonly IHydrator _hydrator;

		public ToListAction(IMetadataStore metadataStore, 
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect, IEnvironmentSettings environment) :
			base(metadataStore, default(TEntity), connection, dialect, environment)
		{
			this._hydrator = hydrator;
		}

		public IEnumerable<TEntity> GetListing(string statement, ICollection<QueryParameter> parameters)
		{
			IEnumerable<TEntity> entities = new List<TEntity>();

			using (var command = this.CreateCommand())
			{
				command.CommandText = statement;
				command.CreateParametersFromQuery(parameters);
                this.DisplayCommand(command);

				if (this._hydrator != null)
				{
					entities = _hydrator.HydrateEntities<TEntity>(command);
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
                this.DisplayCommand(command);

				if (this._hydrator != null)
				{
					entities = _hydrator.HydrateEntities<TEntity>(command);

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