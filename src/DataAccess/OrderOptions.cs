using System;
using System.Linq.Expressions;
using MicroORM.DataAccess.Querying.OrderBy;

namespace MicroORM.DataAccess
{
	public static class OrderOptions
	{
		/// <summary>
		/// Order the resulting data set ascending by a given field.
		/// </summary>
		/// <typeparam name="TEntity">Entity that contains the field to order by</typeparam>
		/// <param name="ascending">Expression that denotes field to order by ascending</param>
		/// <returns></returns>
		public static IOrderOption<TEntity> Asc<TEntity>(Expression<Func<TEntity, object>> ascending)
		{
			return new OrderAscending<TEntity>(ascending);
		}

		/// <summary>
		/// Order the resulting data set descending by a given field.
		/// </summary>
		/// <typeparam name="TEntity">Entity that contains the field to order by</typeparam>
		/// <param name="descending">Expression that denotes field to order by descending</param>
		/// <returns></returns>
		public static IOrderOption<TEntity> Desc<TEntity>(Expression<Func<TEntity, object>> descending)
		{
			return new OrderDescending<TEntity>(descending);
		}
	}
}