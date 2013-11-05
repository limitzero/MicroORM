using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Actions
{
	public class InsertAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		private readonly IHydrator hydrator;

		public InsertAction(IMetadataStore metadataStore, TEntity entity, IHydrator hydrator, SqlConnection connection) :
			base(metadataStore, entity, connection)
		{
			this.hydrator = hydrator;
		}

		public void Insert(TEntity entity)
		{
			using (var command = this.CreateCommand())
			{
				var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();
				var query = tableInfo.GetInsertStatement(entity);
				command.CommandText = query;
				command.CreateAndAddInputParametersForColumns<TEntity>(entity, this.MetadataStore);
				command.DisplayQuery();

				if (this.hydrator != null)
				{
					this.hydrator.InsertEntity<TEntity>(entity, command);
				}
			}
		}
	}
}