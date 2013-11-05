using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Selects.Impl
{
	public class FieldFromEntitySelectOption<TEntity> : ISelectOption
	{
		private readonly Expression<Func<TEntity, object>> field;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public FieldFromEntitySelectOption(Expression<Func<TEntity, object>> field)
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
				string targetField = string.Format("[{0}].[{1}]", table, datacolumn);
				this.Fields = new List<string> {targetField};
			}
		}
	}
}