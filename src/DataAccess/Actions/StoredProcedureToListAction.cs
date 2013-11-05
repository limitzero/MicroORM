using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Actions
{
	public class StoredProcedureToListAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public StoredProcedureToListAction(IMetadataStore metadataStore,
		                                   IHydrator hydrator, SqlConnection connection) :
		                                   	base(metadataStore, default(TEntity), connection)
		{
			this.hydrator = hydrator;
		}

		public IEnumerable<TEntity> GetList(string procedure, IDictionary<string, object> parameters)
		{
			IEnumerable<TEntity> entities = new List<TEntity>();

			using (var command = this.CreateCommand())
			{
				command.CommandText = procedure;
				command.CommandType = CommandType.StoredProcedure;
				command.CreateParametersFromDictionary(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entities = hydrator.HydrateEntities<TEntity>(command);
				}
			}

			return entities;
		}
	}
}