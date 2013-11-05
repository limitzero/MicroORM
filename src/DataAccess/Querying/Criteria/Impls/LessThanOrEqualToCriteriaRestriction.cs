using System;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class LessThanOrEqualToCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public LessThanOrEqualToCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
			string evaluation = MicroORM.Configuration.Instance.Dialect
				.BuildParameterAssignment(DialectComparisonOperator.LessThanOrEqualTo, tableName, dataColumnName);

			//string.Format("[{0}].[{1}] < {2}", tableName, dataColumnName, this.CoalesceValue());
			return evaluation;
		}
	}
}