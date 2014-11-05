using System.Data;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class UpdateAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public UpdateAction(IMetadataStore metadataStore, 
            TEntity entity, IHydrator hydrator, 
            IDbConnection connection, IDialect dialect) :
			base(metadataStore, entity, connection, dialect)
		{
			this.hydrator = hydrator;
		}

		public TEntity Update(TEntity entity)
		{
			using (var command = this.CreateCommand())
			{
				var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

				var query = tableInfo.GetUpdateStatement(entity);
			    var primaryKeyValue = tableInfo.GetPrimaryKeyValue(entity);

			    query = tableInfo.AddWhereClauseById(query, primaryKeyValue);
				command.CreateAndAddInputParameterForPrimaryKey(tableInfo, tableInfo.PrimaryKey, entity);
				command.CreateAndAddInputParametersForColumns(entity, this.MetadataStore);
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