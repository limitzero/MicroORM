using System;
using System.Text;
using Basic.Infrastructure.DataAccess.Internals;

namespace Basic.Infrastructure.DataAccess.Actions
{
	public class InsertDatabaseAction<TEntity> : DatabaseAction<TEntity>
	{
		public InsertDatabaseAction(IMetadataStore metadataStore, TEntity entity) : 
			base(metadataStore, entity)
		{
		}

		public StringBuilder Build(StringBuilder builder)
		{
			if (this.MetadataStore.Entities.ContainsKey(this.Entity.GetType()))
			{
				string table = this.MetadataStore.GetTableName(this.Entity.GetType());
				string statement = string.Format("INSERT INTO [{0}] ", table);
				builder.AppendLine(statement);
				builder.AppendLine(this.BuildFields());
				builder.AppendLine(string.Concat(this.BuildClause(), ";"));
				builder.AppendLine("SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY]");
			}

			return builder;
		}

		private string BuildFields()
		{
			var fields = new StringBuilder();
			var properties = this.MetadataStore.Entities[this.Entity.GetType()];
			var separator = ", ";

			var primarykey = this.MetadataStore.PrimaryKeyField(this.Entity.GetType());

			foreach (var property in properties)
			{
				if (primarykey == property) continue;

				if (property.CanRead)
				{
					var datacolumn = this.MetadataStore.GetColumnNameFromEntityProperty(property);

					if (string.IsNullOrEmpty(datacolumn) == false)
					{
						var propertyValue = property.GetValue(this.Entity, null);

						if (propertyValue == null || propertyValue.ToString() == string.Empty) continue;

						fields.Append(string.Concat(datacolumn, separator));
					}
				}
			}

			string tableColumns = fields.ToString()
				.TrimEnd(string.Concat(separator, Environment.NewLine).ToCharArray()).Trim();

			string fieldlisting = string.Concat("(", tableColumns, ")");

			return fieldlisting;
		}

		private string BuildClause()
		{

			var fields = new StringBuilder();
			var properties = this.MetadataStore.Entities[this.Entity.GetType()];
			var separator = ", ";

			var primarykey = this.MetadataStore.PrimaryKeyField(this.Entity.GetType());

			foreach (var property in properties)
			{
				if (primarykey == property) continue;

				if (property.CanRead)
				{
					var datacolumn = this.MetadataStore.GetColumnNameFromEntityProperty(property);

					if (string.IsNullOrEmpty(datacolumn) == false)
					{
						var propertyValue = property.GetValue(this.Entity, null);

						if (propertyValue == null || propertyValue.ToString() == string.Empty) continue;

						fields.Append(string.Concat(this.CoalesceValue(propertyValue), separator));
					}
				}
			}

			string tableColumns = fields.ToString()
				.TrimEnd(string.Concat(separator, Environment.NewLine).ToCharArray()).Trim();

			string fieldlisting = string.Concat("VALUES" , Environment.NewLine, "(", tableColumns, ")");

			return fieldlisting;
		}
	}
}