using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Selects.Impl
{
	public class CountOnFieldFromEntitySelectOption<TEntity> : ISelectOption
	{
		private readonly Expression<Func<TEntity, object>> field;
		private readonly string columnName;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public CountOnFieldFromEntitySelectOption(Expression<Func<TEntity, object>> field, string columnName)
		{
			this.Fields = new List<string>();
			this.field = field;
			this.columnName = columnName;
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				string property = this.MetadataStore.GetPropertyNameFromExpression(field);
				string datacolumn = this.MetadataStore.GetColumnName(typeof (TEntity), property);
				string table = this.MetadataStore.GetTableName(typeof (TEntity));
				string targetField = string.Format("{0} = count([{1}].[{2}])", this.columnName, table, datacolumn);
				this.Fields = new List<string> {targetField};
			}
		}
	}
}