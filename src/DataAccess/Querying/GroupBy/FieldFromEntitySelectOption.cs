using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.GroupBy
{
	public class FieldFromEntityGroupOption<TEntity> : IGroupByOption
	{
		private readonly Expression<Func<TEntity, object>> field;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public FieldFromEntityGroupOption(Expression<Func<TEntity, object>> field)
		{
			this.Fields = new List<string>();
			this.field = field;
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				string property = this.MetadataStore.GetPropertyNameFromExpression(field);
				string datacolumn = this.MetadataStore.GetColumnName(typeof (TEntity), property);
				string table = this.MetadataStore.GetTableName(typeof (TEntity));
				string targetField = string.Format("{0}.{1}", table, datacolumn);
				this.Fields = new List<string> {targetField};
			}
		}
	}
}