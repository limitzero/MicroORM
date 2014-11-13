using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Criteria.Impls
{
	public class EqualsCriteriaRestriction<TEntity> : BaseCriteriaSelection<TEntity>
	{
		public EqualsCriteriaRestriction(Expression<Func<TEntity, object>> currentExpression, object value)
			: base(currentExpression, value)
		{
		}

		public override string CreateEvaluation(string tableName, string dataColumnName)
		{
			string evaluation = string.Empty;

			if (this.CurrentValue == null)
			{
                //evaluation = MicroORM.Configuration.Impl.EnvironmentConfiguration.Instance.Dialect
                //    .BuildParameterAssignment(DialectComparisonOperator.EqualToNull, tableName, dataColumnName);

				//string.Format("[{0}].[{1}] is null", tableName, dataColumnName);
			}
			else
			{
                //evaluation = MicroORM.Configuration.Impl.EnvironmentConfiguration.Instance.Dialect
                //    .BuildParameterAssignment(DialectComparisonOperator.Equals, tableName, dataColumnName);

				//evaluation = string.Format("[{0}].[{1}] = {2}", tableName, dataColumnName, this.CoalesceValue());
			}

			return evaluation;
		}
	}
}