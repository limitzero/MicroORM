using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class UpdateAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator _hydrator;

		public UpdateAction(IMetadataStore metadataStore, 
            TEntity entity, IHydrator hydrator, 
            IDbConnection connection, IDialect dialect, 
            IEnvironmentSettings environment) :
			base(metadataStore, entity, connection, dialect, environment)
		{
			this._hydrator = hydrator;
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

                this.DisplayCommand(command);

				if (this._hydrator != null)
				{
					entity = this._hydrator.HydrateEntity<TEntity>(command);
				}

				return entity;
			}
		}
	}
}