using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Actions
{
	public class UpdateAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public UpdateAction(IMetadataStore metadataStore, TEntity entity, IHydrator hydrator, SqlConnection connection) :
			base(metadataStore, entity, connection)
		{
			this.hydrator = hydrator;
		}

		public TEntity Update(TEntity entity)
		{
			using (var command = this.CreateCommand())
			{
				var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

				var query = tableInfo.GetUpdateStatement(entity);
				//query = tableInfo.AddWhereClauseById(query, tableInfo.GetPrimaryKeyValue(entity));

				command.CreateAndAddInputParameterForPrimaryKey(tableInfo, tableInfo.PrimaryKey, entity);
				command.CreateAndAddInputParametersForColumns<TEntity>(entity, this.MetadataStore);
				command.CommandText = query;
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					entity = this.hydrator.HydrateEntity<TEntity>(command);
				}

				return entity;
			}
		}
	}
}