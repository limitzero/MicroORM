using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Extensions
{
	public static class IDbCommandExtensions
	{
		public static void CreateAndAddInputParameter(this IDbCommand command,
		    DbType databaseType, string parameterName, object parameterValue)
		{
			var parameter = command.CreateParameter();
			parameter.Direction = ParameterDirection.Input;
			parameter.ParameterName = parameterName;
			parameter.DbType = databaseType;
			parameter.Value = parameterValue;

			TryAddParameterToCollection(command, parameter);
		}

        public static void CreateAndAddInputParameterForPrimaryKey(this IDbCommand command,
		    TableInfo tableinfo, PrimaryKeyInfo primaryKeyInfo,
		     object entity)
		{
			var parameter = command.CreateParameter();
			parameter.Direction = ParameterDirection.Input;
			parameter.ParameterName = primaryKeyInfo.GetPrimaryKeyName();
			parameter.DbType = primaryKeyInfo.DbType;
			parameter.Value = tableinfo.CoalesceValue(primaryKeyInfo.Column.GetValue(entity, null));

			TryAddParameterToCollection(command, parameter);
		}

        public static void CreateAndAddInputParameterForPrimaryKeyValue(this IDbCommand command,
           TableInfo tableinfo, PrimaryKeyInfo primaryKeyInfo,
            object value)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = primaryKeyInfo.GetPrimaryKeyName();
            parameter.DbType = primaryKeyInfo.DbType;
            parameter.Value = tableinfo.CoalesceValue(value);

            TryAddParameterToCollection(command, parameter);
        }

		public static void CreateAndAddInputParametersForColumns<TEntity>(this IDbCommand command,
		    TEntity entity, IMetadataStore metadataStore)
			where TEntity : class
		{
			var tableInfo = metadataStore.GetTableInfo<TEntity>();

			// entity attributes (regular):
			foreach (var columnInfo in tableInfo.Columns)
			{
				if (columnInfo.Column.GetValue(entity, null) != null)
				{
					CreateAndAddInputParameterInternal(command, tableInfo, entity, columnInfo);
					//CreateAndAddInputParameterForPrimaryKey(command, tableInfo, columnInfo, entity);
				}
			}

			// entity components (expanded data attributes):
			foreach (var component in tableInfo.Components)
			{
				if (component.Column.GetValue(entity, null) != null)
				{
					metadataStore.AddEntity(component.Column.PropertyType);
					var componentTableInfo = metadataStore.GetTableInfo(component.Column.PropertyType);
					var theComponent = entity.GetType().GetProperty(component.Column.Name).GetValue(entity, null);

					if (theComponent != null)
					{
						foreach (var column in componentTableInfo.Columns)
						{
							if (column.Column.GetValue(theComponent, null) != null)
							{
								CreateAndAddInputParameterInternal(command, componentTableInfo, theComponent, column);
							}
						}
					}
				}
			}

			// entity references:
			foreach (var reference in tableInfo.References)
			{
				var property = entity.GetType().GetProperty(reference.Column.Name);
				var parent = property.GetValue(entity, null);

				if (parent != null)
				{
					var parentPrimaryKeyTableInfo = metadataStore.GetTableInfo(parent.GetType());

					CreateAndAddInputParameterForPrimaryKey(command,
					                                        parentPrimaryKeyTableInfo,
					                                        parentPrimaryKeyTableInfo.PrimaryKey,
					                                        parent);
				}
			}
		}


		public static void CreateParametersFromDictionary(this IDbCommand command, IDictionary<string, object> parameters)
		{
			foreach (var parameter in parameters)
			{
				var sqlParameter = command.CreateParameter();
				sqlParameter.Direction = ParameterDirection.Input;
				sqlParameter.ParameterName = parameter.Key;
				sqlParameter.DbType = TypeConverter.ConvertToDbType(parameter.Value.GetType());
				sqlParameter.Value = parameter.Value;

				TryAddParameterToCollection(command, sqlParameter);
			}
		}

		public static void CreateParametersFromQuery(this IDbCommand command, ICollection<QueryParameter> parameters)
		{
			foreach (var queryParameter in parameters)
			{
				CreateAndAddInputParameter(command,
				                           queryParameter.Property.DbType,
				                           queryParameter.Property.DataColumnName,
				                           queryParameter.Value);
			}
		}

		public static void DisplayQuery(this IDbCommand command)
		{
#if DEBUG
			var builder = new StringBuilder();
			builder.Append(command.CommandText).AppendLine(";");

			for (int index = 0; index < command.Parameters.Count; index++)
			{
			    var parameter = command.Parameters[index] as DbParameter;
                
				builder.Append(string.Format("[{0} = {1}], ",
				       string.Concat("@", parameter.ParameterName), parameter.Value));
			}

			string parameters = builder.ToString().TrimEnd(string.Concat(", ").ToCharArray());
			System.Diagnostics.Debug.WriteLine(string.Concat("MicroORM : ", parameters));
#endif
		}

		private static void TryAddParameterToCollection(IDbCommand command, IDbDataParameter parameter)
		{
			bool isFound = false;

			for (int index = 0; index < command.Parameters.Count; index++)
			{
			    var currentParameter = command.Parameters[index] as DbParameter;
                if ( !currentParameter.ParameterName.Equals(parameter.ParameterName) )
                    continue;

				isFound = true;
				break;
			}

			if (isFound == true) return;
			command.Parameters.Add(parameter);
		}

		private static void CreateAndAddInputParameterInternal(IDbCommand command,
		    TableInfo tableInfo, object entity, ColumnInfo columnInfo)
		{
			var parameter = command.CreateParameter();
			parameter.Direction = ParameterDirection.Input;
			parameter.ParameterName = columnInfo.DataColumnName;
			parameter.DbType = columnInfo.DbType;
			parameter.Value = tableInfo.CoalesceValue(columnInfo.Column.GetValue(entity, null));

			TryAddParameterToCollection(command, parameter);
		}
	}
}