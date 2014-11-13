using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class InsertAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator _hydrator;

		public InsertAction(IMetadataStore metadataStore, TEntity entity, 
            IHydrator hydrator, IDbConnection connection, 
            IDialect dialect, IEnvironmentSettings environment) :
			base(metadataStore, entity, connection, dialect, environment)
		{
			this._hydrator = hydrator;
		}

		public void Insert(TEntity entity)
		{
			using (var command = this.CreateCommand())
			{
				var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

				var insert = tableInfo.GetInsertStatement(entity);
			    var identity = this.Dialect.GetIdentityStatement(tableInfo.PrimaryKey);

			    var query = string.Format("{0};{1}", insert, identity);

				command.CommandText = query;
				command.CreateAndAddInputParametersForColumns<TEntity>(entity, this.MetadataStore);

                this.DisplayCommand(command);

				if (this._hydrator != null)
				{
					this._hydrator.InsertEntity<TEntity>(entity, command);
				}
			}
		}
	}
}