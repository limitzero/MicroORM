using System;
using System.Text;
using Basic.Infrastructure.DataAccess.Internals;

namespace Basic.Infrastructure.DataAccess.Actions
{
	public class UpdateDatabaseAction<TEntity> : DatabaseAction<TEntity>
	{
		public UpdateDatabaseAction(IMetadataStore metadataStore, TEntity entity)
			: base(metadataStore, entity)
		{
		}

		public StringBuilder Build(StringBuilder builder)
		{
			if (this.MetadataStore.Entities.ContainsKey(this.Entity.GetType()))
			{
				string table = this.MetadataStore.GetTableName(this.Entity.GetType());
				string statement = string.Format("UPDATE [{0}] SET", table);
				builder.AppendLine(statement);
				builder.AppendLine(this.BuildFields());
				builder.AppendLine(this.BuildClause());
			}

			return builder;
		}

		private string BuildFields()
		{
			var fields = new StringBuilder();
			var properties = this.MetadataStore.Entities[this.Entity.GetType()];
			var fieldformat = "[{0}] = {1}{2}";
			var separator = ", ";

			var primarykey = this.MetadataStore.PrimaryKeyField(this.Entity.GetType());

			foreach (var property in properties)
			{
				if(primarykey == property) continue;

				if (property.CanRead)
				{
					var datacolumn = this.MetadataStore.GetColumnNameFromEntityProperty(property);
					var propertyValue = property.GetValue(this.Entity, null);

					if(propertyValue == null || propertyValue.ToString() == string.Empty) continue;

					fields.AppendLine(string.Format(fieldformat, datacolumn,
					                                base.CoalesceValue(propertyValue), separator));
				}
			}
			return fields.ToString().TrimEnd(string.Concat(separator,Environment.NewLine).ToCharArray()).Trim();
		}

		public string BuildClause()
		{
			string clause = "WHERE [{0}] = {1}";
			var primarykey = this.MetadataStore.PrimaryKeyField(this.Entity.GetType());
			var columnName = this.MetadataStore.GetColumnNameFromEntityProperty(primarykey);
			var data = primarykey.GetValue(this.Entity, null);
			return string.Format(clause, columnName, data.ToString());
		}
	}
}