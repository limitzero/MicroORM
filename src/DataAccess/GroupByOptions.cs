using System;
using System.Linq.Expressions;
using MicroORM.DataAccess.Querying.GroupBy;

namespace MicroORM.DataAccess
{
	public static class GroupByOptions
	{
		public static IGroupByOption FieldFrom<TEntity>(Expression<Func<TEntity, object>> field)
		{
			return new FieldFromEntityGroupOption<TEntity>(field);
		}

		public static IGroupByOption AliasField<TEntity>(string field)
		{
			return new AliasFieldFromEntityGroupOption<TEntity>(field);
		}
	}
}