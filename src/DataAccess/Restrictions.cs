using System;
using System.Linq.Expressions;
using MicroORM.DataAccess.Querying.Criteria;
using MicroORM.DataAccess.Querying.Criteria.Impls;

namespace MicroORM.DataAccess
{
	public static class Restrictions
	{
		/// <summary>
		/// Logical "AND" condition
		/// </summary>
		/// <returns></returns>
		public static ICriteriaRestriction Including(params ICriteriaRestriction[] restrictions)
		{
			return new AndCriteriaSelection(restrictions);
		}

		/// <summary>
		/// Logical "OR" condition
		/// </summary>
		/// <returns></returns>
		public static ICriteriaRestriction CreateDisjunctionOn(params ICriteriaRestriction[] restrictions)
		{
			return new OrCriteriaSelection(restrictions);
		}

		public static ICriteriaRestriction EqualTo<TEntity>(Expression<Func<TEntity, object>> expression, object value)
		{
			return new EqualsCriteriaRestriction<TEntity>(expression, value);
		}

		public static ICriteriaRestriction NotEqualTo<TEntity>(Expression<Func<TEntity, object>> expression, object value)
		{
			return new NotEqualsCriteriaRestriction<TEntity>(expression, value);
		}

		public static ICriteriaRestriction LessThan<TEntity>(Expression<Func<TEntity, object>> expression, object value)
		{
			return new LessThanCriteriaRestriction<TEntity>(expression, value);
		}

		public static ICriteriaRestriction LessThanOrEqualTo<TEntity>(Expression<Func<TEntity, object>> expression,
		                                                              object value)
		{
			return new LessThanOrEqualToCriteriaRestriction<TEntity>(expression, value);
		}

		public static ICriteriaRestriction GreaterThan<TEntity>(Expression<Func<TEntity, object>> expression, object value)
		{
			return new GreaterThanCriteriaRestriction<TEntity>(expression, value);
		}

		public static ICriteriaRestriction GreaterThanOrEqualTo<TEntity>(Expression<Func<TEntity, object>> expression,
		                                                                 object value)
		{
			return new GreaterThanOrEqualToCriteriaRestriction<TEntity>(expression, value);
		}

		/// <summary>
		/// Marker restriction for the SQL "like" clause.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="expression"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static ICriteriaRestriction Like<TEntity>(Expression<Func<TEntity, object>> expression, object value)
		{
			return new LikeCriteriaRestriction<TEntity>(expression, value);
		}

		/// <summary>
		/// Marker restriction for the SQL "in" clause.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="expression"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static ICriteriaRestriction In<TEntity>(Expression<Func<TEntity, object>> expression, params object[] values)
		{
			throw new NotImplementedException("'in' data restriction clause not implemented");
		}
	}
}