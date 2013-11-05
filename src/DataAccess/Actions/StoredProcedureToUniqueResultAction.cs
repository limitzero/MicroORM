using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Actions
{
	public class StoredProcedureToUniqueResultAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public StoredProcedureToUniqueResultAction(IMetadataStore metadataStore,
		                                           IHydrator hydrator,
		                                           SqlConnection connection)
			: base(metadataStore, default(TEntity), connection)
		{
			this.hydrator = hydrator;
		}

		public TEntity GetUniqueResult(string procedure, IDictionary<string, object> parameters)
		{
			TEntity entity = default(TEntity);

			using (var command = this.CreateCommand())
			{
				command.CommandText = procedure;
				command.CommandType = CommandType.StoredProcedure;
				command.CreateParametersFromDictionary(parameters);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entity = hydrator.HydrateEntity<TEntity>(command);
				}
			}

			return entity;
		}
	}
}