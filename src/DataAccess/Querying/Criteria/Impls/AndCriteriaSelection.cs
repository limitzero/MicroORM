using System;
using System.Collections.Generic;
using System.Text;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class AndCriteriaSelection : ICriteriaRestriction
	{
		private readonly List<ICriteriaRestriction> restrictions;
		private List<QueryParameter> parameters;

		public ICollection<string> Expressions { get; set; }
		public IMetadataStore MetadataStore { get; set; }
		public QueryParameter Parameter { get; set; }

		public AndCriteriaSelection()
		{
			this.parameters = new List<QueryParameter>();
			this.restrictions = new List<ICriteriaRestriction>();
			this.Expressions = new List<string>();
		}

		public AndCriteriaSelection(params ICriteriaRestriction[] restrictions)
			: this()
		{
			this.restrictions = new List<ICriteriaRestriction>(restrictions);
		}

		public void AddRestriction(ICriteriaRestriction restriction)
		{
			if (!this.restrictions.Contains(restriction))
				this.restrictions.Add(restriction);
		}

		public void Build()
		{
			var builder = new StringBuilder();
			var conditions = new List<string>();
			const string conjuction = "and";

			foreach (var criteriaRestriction in restrictions)
			{
				criteriaRestriction.MetadataStore = this.MetadataStore;
				criteriaRestriction.Build();

				if (criteriaRestriction.Parameter != null)
					this.parameters.Add(criteriaRestriction.Parameter);

				conditions.AddRange(criteriaRestriction.Expressions);
			}


			foreach (var condition in conditions)
			{
				builder.AppendLine(conjuction).AppendLine(condition);
			}

			var expression = builder.ToString().TrimEnd(string.Concat(System.Environment.NewLine).ToCharArray());
			this.Expressions.Add(expression);
		}

		public ICollection<QueryParameter> GetParameters()
		{
			return this.parameters;
		}
	}
}