using System;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class LikeCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public LikeCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
			string evaluation = string.Empty;

			if (this.CurrentValue == null) return evaluation;

			// remove the current parameter creation as we will need to use the literal value:
			this.Parameter = null;

			string literal = this.CoalesceValue().Replace("'", string.Empty);

            evaluation = MicroORM.Configuration.Impl.Configuration.Instance.Dialect
				.BuildParameterAssignment(DialectComparisonOperator.Like, tableName, dataColumnName, literal);

			// string.Format("[{0}].[{1}] like '%{2}%'", tableName, dataColumnName, this.CoalesceValue().Replace("'", string.Empty));

			return evaluation;
		}
	}
}