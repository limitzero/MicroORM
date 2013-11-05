using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.OrderBy
{
	public class OrderAscending<TEntity> : IOrderOption<TEntity>
	{
		private readonly Expression<Func<TEntity, object>> @ascending;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public OrderAscending(Expression<Func<TEntity, object>> @ascending)
		{
			this.@ascending = ascending;
			this.Fields = new List<string>();
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				string property = this.MetadataStore.GetPropertyNameFromExpression(this.ascending);
				string datacolumn = this.MetadataStore.GetColumnName(typeof (TEntity), property);
				string table = this.MetadataStore.GetTableName(typeof (TEntity));
				string targetField = string.Format("{0}.{1} asc", table, datacolumn);
				this.Fields = new List<string> {targetField};
			}
		}
	}
}