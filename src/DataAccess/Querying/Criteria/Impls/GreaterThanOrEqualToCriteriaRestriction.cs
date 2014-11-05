using System;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class GreaterThanOrEqualToCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public GreaterThanOrEqualToCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
            string evaluation = MicroORM.Configuration.Impl.Configuration.Instance.Dialect
				.BuildParameterAssignment(DialectComparisonOperator.GreaterThanOrEqualTo, tableName, dataColumnName);

			//string evaluation = string.Format("[{0}].[{1}] > {2}", tableName, dataColumnName, this.CoalesceValue());
			return evaluation;
		}
	}
}