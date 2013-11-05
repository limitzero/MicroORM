using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.OrderBy
{
	public class OrderDescending<TEntity> : IOrderOption<TEntity>
	{
		private readonly Expression<Func<TEntity, object>> @descending;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public OrderDescending(Expression<Func<TEntity, object>> @descending)
		{
			this.descending = descending;
			this.Fields = new List<string>();
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				string property = this.MetadataStore.GetPropertyNameFromExpression(this.descending);
				string datacolumn = this.MetadataStore.GetColumnName(typeof (TEntity), property);
				string table = this.MetadataStore.GetTableName(typeof (TEntity));
				string targetField = string.Format("{0}.{1} desc", table, datacolumn);
				this.Fields = new List<string> {targetField};
			}
		}
	}
}