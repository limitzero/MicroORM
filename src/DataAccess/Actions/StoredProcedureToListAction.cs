using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class StoredProcedureToListAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator _hydrator;

		public StoredProcedureToListAction(IMetadataStore metadataStore,
		   IHydrator hydrator, IDbConnection connection, 
            IDialect dialect, IEnvironmentSettings environment) 
		    : base(metadataStore, default(TEntity), connection, dialect, environment)
		{
			this._hydrator = hydrator;
		}

		public IEnumerable<TEntity> GetList(string procedure, IDictionary<string, object> parameters)
		{
			IEnumerable<TEntity> entities = new List<TEntity>();

			using (var command = this.CreateCommand())
			{
				command.CommandText = procedure;
				command.CommandType = CommandType.StoredProcedure;
				command.CreateParametersFromDictionary(parameters);

				// command.DisplayQuery();
                this.DisplayCommand(command);

				if (this._hydrator != null)
				{
					entities = _hydrator.HydrateEntities<TEntity>(command);
				}
			}

			return entities;
		}
	}
}