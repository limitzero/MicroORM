using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public abstract class BaseCriteriaSelection<TEntity> : ICriteriaRestriction
	{
		private readonly Expression<Func<TEntity, object>> currentExpression;
		public ICollection<string> Expressions { get; set; }
		public IMetadataStore MetadataStore { get; set; }
		public QueryParameter Parameter { get; set; }
		protected object CurrentValue { get; private set; }

		protected BaseCriteriaSelection(Expression<Func<TEntity, object>> expression, object value)
		{
			this.CurrentValue = value;
			this.currentExpression = expression;
			this.Expressions = new List<string>();
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();

				string property = this.MetadataStore.GetPropertyNameFromExpression(this.currentExpression);
				string datacolumn = this.MetadataStore.GetColumnName(typeof (TEntity), property);
				string table = tableinfo.TableName;

				string evaluation = this.CreateEvaluation(table, datacolumn);

				var column = tableinfo.FindColumnForProperty(property);
				this.Parameter = new QueryParameter(typeof (TEntity), column, tableinfo.CoalesceValue(this.CurrentValue),
				                                    this.CurrentValue);

				if (string.IsNullOrEmpty(evaluation) == false)
					this.Expressions = new List<string> {evaluation};
			}
		}

		public abstract string CreateEvaluation(string tableName, string dataColumnName);

		protected string CoalesceValue()
		{
			return this.MetadataStore.GetTableInfo<TEntity>().CoalesceValue(this.CurrentValue);
		}
	}
}