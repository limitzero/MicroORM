using System;
using System.Linq.Expressions;
using MicroORM.DataAccess.Querying.Selects;
using MicroORM.DataAccess.Querying.Selects.Impl;

namespace MicroORM.DataAccess
{
	public static class SelectionOptions
	{
		public static ISelectOption AllFrom<TEntity>()
		{
			return new AllFromEntitySelectOption<TEntity>();
		}

		public static ISelectOption FieldFrom<TEntity>(Expression<Func<TEntity, object>> field)
		{
			return new FieldFromEntitySelectOption<TEntity>(field);
		}

		/// <summary>
		/// This will compute the aggregate function "count" on the indicated field.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="field"></param>
		/// <param name="computedColumnName">The name of the field to hold the aggregate information</param>
		/// <returns></returns>
		public static ISelectOption CountOnFieldFrom<TEntity>(Expression<Func<TEntity, object>> field,
		                                                      string computedColumnName)
		{
			return new CountOnFieldFromEntitySelectOption<TEntity>(field, computedColumnName);
		}

		/// <summary>
		/// This will compute the aggregate function "max" on the indicated field.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="field"></param>
		/// <param name="computedColumnName">The name of the field to hold the aggregate information</param>
		/// <returns></returns>
		public static ISelectOption MaxOnFieldFrom<TEntity>(Expression<Func<TEntity, object>> field, string computedColumnName)
		{
			return new MaxOnFieldFromEntitySelectOption<TEntity>(field, computedColumnName);
		}

		/// <summary>
		/// This will compute the aggregate function "avg" on the indicated field.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="field"></param>
		/// <param name="computedColumnName">The name of the field to hold the aggregate information</param>
		/// <returns></returns>
		public static ISelectOption AvgOnFieldFrom<TEntity>(Expression<Func<TEntity, object>> field, string computedColumnName)
		{
			return new AverageOnFieldFromEntitySelectOption<TEntity>(field, computedColumnName);
		}
	}
}