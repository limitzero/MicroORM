using System;
using System.Collections;
using System.Data;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.Actions
{
	public class InitializeProxyAction : DatabaseAction
	{
		public InitializeProxyAction(IMetadataStore metadataStore,
		  IHydrator hydrator, IDbConnection connection)
		  : base(metadataStore, hydrator, connection)
		{
		}

		public void InitializeProxy(object entity, string targetPropertyName, Type targetType)
		{
            using ( var command = this.Connection.CreateCommand() )
			{
				// get the identifier of the parent "entity" and use the 
				// primary key field value as the key into the child object(s);
				var parent = this.MetadataStore.GetTableInfo(entity.GetType());
				Type childPropertyType = entity.GetType().GetProperty(targetPropertyName).PropertyType;

				Type childType = null;
				TableInfo child = null;

				if (targetType.IsGenericType &&
				    typeof (IEnumerable).IsAssignableFrom(targetType))
				{
					// at this point we are proxying a collection:
					// make sure to hydrate all instances and set 
					// the proxied instance back on the parent entity:
					childType = targetType.GetGenericArguments()[0];
					this.MetadataStore.AddEntity(childType);
					child = this.MetadataStore.GetTableInfo(childType);
				}
				else
				{
					// at this point we are proxying a reference type 
					// as a child entity on the parent:
					childType = targetType;
					this.MetadataStore.AddEntity(childType);
					child = this.MetadataStore.GetTableInfo(targetType);
				}

				var query = child.GetSelectStatmentForAllFields();
				object id = parent.PrimaryKey.Column.GetValue(entity, null);
				query = child.AddWhereClauseForParentById(parent.PrimaryKey, query, id);

				command.CommandText = query;
				command.CreateAndAddInputParameterForPrimaryKey(parent, parent.PrimaryKey, entity);
				command.DisplayQuery();

				Hydrator.UpdateEntity(childPropertyType, childType, entity, command);
			}
		}
	}
}