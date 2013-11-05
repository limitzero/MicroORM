using System.Data.SqlClient;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Actions
{
	public class DeleteAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		public DeleteAction(IMetadataStore metadataStore, TEntity entity, SqlConnection connection) :
			base(metadataStore, entity, connection)
		{
		}

		public void Delete(TEntity entity)
		{
			using (var command = this.CreateCommand())
			{
				var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();

				// guard on "open" delete statements:
				if (tableinfo.IsPrimaryKeySet(entity) == false) return;

				var query = tableinfo.GetDeleteStatement();

				object id = tableinfo.PrimaryKey.Column.GetValue(entity, null);

				//query = tableinfo.AddWhereClauseById(query, tableinfo.GetPrimaryKeyValue(entity));

				command.CommandText = query;
				command.CreateAndAddInputParameter(tableinfo.PrimaryKey.DbType, tableinfo.PrimaryKey.DataColumnName, id);
				command.DisplayQuery();

				if (command.Connection != null)
					command.ExecuteNonQuery();
			}
		}
	}
}