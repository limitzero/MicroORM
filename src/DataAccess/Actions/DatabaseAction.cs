using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.Actions
{
	public abstract class DatabaseAction
	{
		protected IMetadataStore MetadataStore { get; private set; }
		protected IHydrator Hydrator { get; private set; }
		protected SqlConnection Connection { get; private set; }

		protected DatabaseAction(IMetadataStore metadataStore,
		                         IHydrator hydrator,
		                         SqlConnection connection)
		{
			this.MetadataStore = metadataStore;
			this.Hydrator = hydrator;
			Connection = connection;
		}

		protected SqlCommand CreateCommand()
		{
			var command = new SqlCommand();
			command.Connection = this.Connection;
			command.CommandType = CommandType.Text;
			return command;
		}
	}

	public abstract class DatabaseAction<TEntity>
	{
		private readonly SqlConnection connection;
		protected IMetadataStore MetadataStore { get; private set; }
		protected TEntity Entity { get; set; }

		protected DatabaseAction(IMetadataStore metadataStore, TEntity entity, SqlConnection connection)
		{
			this.connection = connection;
			this.MetadataStore = metadataStore;
			Entity = entity;
		}

		protected SqlCommand CreateCommand()
		{
			var command = new SqlCommand();
			command.Connection = this.connection;
			command.CommandType = CommandType.Text;
			return command;
		}

		protected void DisplayCommandQuery(SqlCommand command)
		{
#if DEBUG
			var builder = new StringBuilder();
			builder.Append(command.CommandText).AppendLine(";");

			for (int index = 0; index < command.Parameters.Count; index++)
			{
				var parameter = command.Parameters[index];
				builder.AppendLine(string.Format("{0} = {1},", string.Concat("@", parameter.ParameterName), parameter.Value));
			}

			string parameters = builder.ToString().TrimEnd(string.Concat(",", System.Environment.NewLine).ToCharArray());
			System.Diagnostics.Debug.WriteLine(parameters);
#endif
		}

		protected string GetParametersWithValueAssigmment(object entity)
		{
			var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();
			var builder = new StringBuilder();

			foreach (var columnInfo in tableinfo.Columns)
			{
				if (columnInfo is PrimaryKeyInfo) continue;
				{
					if (columnInfo.Column.CanRead && !columnInfo.Column.PropertyType.IsGenericType)
					{
						var data = this.CoalesceValue(columnInfo.Column.GetValue(entity, null));
						builder.Append(string.Format("@{0} = {1},", columnInfo.DataColumnName, data));
					}
				}
			}

			return builder.ToString().TrimEnd(",".ToCharArray());
		}

		protected string GetColumnsWithParameterAssignment(object enity)
		{
			var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();
			var builder = new StringBuilder();

			foreach (var columnInfo in tableinfo.Columns)
			{
				if (columnInfo is PrimaryKeyInfo) continue;
				{
					if (columnInfo.Column.CanRead && !columnInfo.Column.PropertyType.IsGenericType)
					{
						builder.Append(string.Format("[{0}] = @{1},", columnInfo.DataColumnName, columnInfo.DataColumnName));
					}
				}
			}

			return builder.ToString().TrimEnd(",".ToCharArray());
		}

		protected string CoalesceValue(object value)
		{
			string newValue = "is null";
			DateTime dateTime;
			var isDateParsed = DateTime.TryParse(value.ToString(), out dateTime);

			if (typeof (string).IsAssignableFrom(value.GetType()))
			{
				newValue = string.Format("'{0}'", value.ToString());
			}
			else if (typeof (DateTime).IsAssignableFrom(value.GetType()) ||
			         isDateParsed == true)
			{
				if (isDateParsed == false)
				{
					dateTime = (DateTime) value;
				}

				newValue = string.Format("'{0}'", dateTime.ToShortDateString());
			}
			else if (value == null)
			{
				newValue = "null";
			}
			else
			{
				newValue = value.ToString();
			}
			return newValue;
		}
	}
}