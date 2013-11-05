using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using MicroORM.DataAccess.Querying.Criteria;
using MicroORM.DataAccess.Querying.GroupBy;
using MicroORM.DataAccess.Querying.OrderBy;
using MicroORM.DataAccess.Querying.Selects;

namespace MicroORM.DataAccess.Querying
{
	/// <summary>
	/// Contract for creating entity queries based on attribute mappings.
	/// </summary>
	/// <typeparam name="TParentEntity">Entity to query on</typeparam>
	public interface IQuery<TParentEntity> where TParentEntity : class, new()
	{
		/// <summary>
		/// Gets the current statement to be executed agains the data store:
		/// </summary>
		string CurrentStatement { get; }

		/// <summary>
		/// This will cause an inner join between the indicated entity and the parent entity 
		/// on a given field via expression.
		/// </summary>
		/// <typeparam name="TChildEntity">Entity to join on</typeparam>
		/// <param name="child">Expression for join entity field</param>
		/// <param name="parent">Expression for parent entity field</param>
		/// <returns></returns>
		IQuery<TParentEntity> JoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
		                                           Expression<Func<TParentEntity, object>> parent)
			where TChildEntity : class, new();

		/// <summary>
		/// This will cause an outer join between the indicated entity and the parent entity 
		/// on a given field via expression.
		/// </summary>
		/// <typeparam name="TChildEntity">Entity to join on</typeparam>
		/// <param name="child">Expression for join entity field</param>
		/// <param name="parent">Expression for parent entity field</param>
		/// <returns></returns>
		IQuery<TParentEntity> OuterJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
		                                                Expression<Func<TParentEntity, object>> parent)
			where TChildEntity : class, new();

		/// <summary>
		/// This will cause an left join between the indicated entity and the parent entity 
		/// on a given field via expression.
		/// </summary>
		/// <typeparam name="TChildEntity">Entity to join on</typeparam>
		/// <param name="child">Expression for join entity field</param>
		/// <param name="parent">Expression for parent entity field</param>
		/// <returns></returns>
		IQuery<TParentEntity> LeftJoinOn<TChildEntity>(Expression<Func<TChildEntity, object>> child,
		                                               Expression<Func<TParentEntity, object>> parent)
			where TChildEntity : class, new();

		/// <summary>
		/// This will build the "where" clause of the query based on restrictions for entity 
		/// field values.
		/// </summary>
		/// <param name="restrictions"></param>
		/// <returns></returns>
		IQuery<TParentEntity> CreateCriteria(params ICriteriaRestriction[] restrictions);

		/// <summary>
		/// This will allow for an "eager fetch" on an entity collection.
		/// </summary>
		/// <param name="eagerFetch"></param>
		/// <returns></returns>
		IQuery<TParentEntity> EagerFetch(Expression<Func<TParentEntity, IEnumerable>> eagerFetch);

		/// <summary>
		/// This will denote the select options for fields to be included in the query. By 
		/// default all of the fields from the parent entity will be included.
		/// </summary>
		/// <param name="selections">Selection option for added fields</param>
		/// <returns></returns>
		IQuery<TParentEntity> Select(params ISelectOption[] selections);

		/// <summary>
		/// This will create the ordering of the resultant data by an expression for 
		/// an entity included in the construction of the query.
		/// </summary>
		/// <typeparam name="TEntity">Entity to base the ordering of the resultset</typeparam>
		/// <param name="order">Orderting option for data.</param>
		/// <returns></returns>
		IQuery<TParentEntity> AddOrder<TEntity>(IOrderOption<TEntity> order);

		/// <summary>
		/// This will create the grouping of the resultant data by the selected
		/// fields within the constructed query.
		/// </summary>
		/// <param name="selections"></param>
		/// <returns></returns>
		IQuery<TParentEntity> GroupBy(params IGroupByOption[] selections);


		/// <summary>
		/// This will return a single entity that matches the criteria defined or a null instance.
		/// </summary>
		/// <returns></returns>
		TParentEntity SingleOrDefault();

		/// <summary>
		/// This will return an entity from the query that is different from 
		/// the parent entity that the query was initiated for (essentially 
		/// map the data into another entity).
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		TEntity SingleOrDefault<TEntity>() where TEntity : class;

		/// <summary>
		/// This will return a listing of objects from the configured 
		/// query with a default maximum result set defined.
		/// </summary>
		/// <param name="maxResults">Maximum results returned in query. Default is 100</param>
		/// <returns></returns>
		IEnumerable<TParentEntity> ToList(int maxResults = 100);


		/// <summary>
		/// This will return a set of entities from the query that is different from 
		/// the parent entity that the query was initiated for (essentially 
		/// map the data into another entity).
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		IEnumerable<TEntity> ToList<TEntity>(int maxResults = 100) where TEntity : class;
	}
}