using System.Collections.Generic;
using System.Text;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class OrCriteriaSelection : ICriteriaRestriction
	{
		private readonly ICriteriaRestriction[] restrictions;
		private List<QueryParameter> parameters;

		public ICollection<string> Expressions { get; set; }
		public QueryParameter Parameter { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public OrCriteriaSelection(params ICriteriaRestriction[] restrictions)
		{
			this.parameters = new List<QueryParameter>();
			this.restrictions = restrictions;
			this.Expressions = new List<string>();
		}

		public void Build()
		{
			var builder = new StringBuilder();
			var conditions = new List<string>();
			const string conjuction = "or";

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
				builder.AppendLine(condition).AppendLine(conjuction);
			}

			var expression = builder.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
			expression = expression.TrimEnd(conjuction.ToCharArray());

			this.Expressions.Add(string.Concat("(", expression, ")"));
		}

		public ICollection<QueryParameter> GetParameters()
		{
			return this.parameters;
		}
	}
}