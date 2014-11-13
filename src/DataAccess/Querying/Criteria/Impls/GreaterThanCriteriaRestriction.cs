using System;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class GreaterThanCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public GreaterThanCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
            var evaluation = string.Empty; 

            //string evaluation = MicroORM.Configuration.Impl.EnvironmentConfiguration.Instance.Dialect
            //    .BuildParameterAssignment(DialectComparisonOperator.GreaterThan, tableName, dataColumnName);

			//string evaluation = string.Format("[{0}].[{1}] > {2}", tableName, dataColumnName, this.CoalesceValue());
			return evaluation;
		}
	}
}