﻿using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
	public class DeleteAction<TEntity> : DatabaseAction<TEntity>
		where TEntity : class
	{
		public DeleteAction(IMetadataStore metadataStore, 
            TEntity entity, IDbConnection connection, 
            IDialect dialect, IEnvironmentSettings environment) :
			base(metadataStore, entity, connection, dialect, environment)
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

				//query = tableinfo.AddWhereClauseById(query, id);

				command.CommandText = query;
                command.CreateAndAddInputParameterForPrimaryKey(tableinfo, tableinfo.PrimaryKey, entity);
				//command.CreateAndAddInputParameter(tableinfo.PrimaryKey.DbType, tableinfo.PrimaryKey.GetPrimaryKeyName(), id);
				//command.DisplayQuery();

                this.DisplayCommand(command);

				if (command.Connection != null)
					command.ExecuteNonQuery();
			}
		}
	}
}