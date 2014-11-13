using System;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class NotEqualsCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public NotEqualsCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
			string evaluation = string.Empty;

			if (this.CurrentValue == null)
			{
                //evaluation = MicroORM.Configuration.Impl.EnvironmentConfiguration.Instance.Dialect
                //    .BuildParameterAssignment(DialectComparisonOperator.NotEqualToNull, tableName, dataColumnName);

				// string.Format("[{0}].[{1}] is not null", tableName, dataColumnName);
			}
			else
			{
                //evaluation = MicroORM.Configuration.Impl.EnvironmentConfiguration.Instance.Dialect
                //    .BuildParameterAssignment(DialectComparisonOperator.NotEquals, tableName, dataColumnName);

				// string.Format("[{0}].[{1}] <> {2}", tableName, dataColumnName, this.CoalesceValue());
			}

			return evaluation;
		}
	}
}