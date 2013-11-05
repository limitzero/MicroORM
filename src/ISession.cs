using System;
using System.Linq;
using System.Text;
using LinqExtender;
using MicroORM.DataAccess.Querying;

namespace MicroORM
{
	/// <summary>
	/// Contract for interacting with the persistance store.
	/// </summary>
	public interface ISession : IDisposable
	{
		/// <summary>
		/// This will fetch a new instance of the entity from the persistance store by identifier.
		/// </summary>
		/// <typeparam name="TEntity">Entity to fetch</typeparam>
		/// <param name="id">Identifier value to base fetch on.</param>
		/// <returns></returns>
		TEntity Get<TEntity>(object id) where TEntity : class;

		/// <summary>
		/// This will fetch a created instance of the entity from the session cache store by identifier.
		/// </summary>
		/// <typeparam name="TEntity">Entity to fetch</typeparam>
		/// <param name="id">Identifier value to base fetch on.</param>
		/// <returns></returns>
		TEntity Load<TEntity>(object id) where TEntity : class;

		/// <summary>
		/// This will perform an update on the entity if the primary key value 
		/// is set, otherwise it will defer to an insert operation if the primary 
		/// key value is not set.
		/// </summary>
		/// <typeparam name="TEntity">Entity to save</typeparam>
		/// <param name="entity">Entity instance</param>
		void Save<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>
		/// This will perform an update on the entity if the primary key value 
		/// is set, otherwise it will defer to an insert operation if the primary 
		/// key value is not set.
		/// </summary>
		/// <typeparam name="TEntity">Entity to save</typeparam>
		/// <param name="entity">Entity instance</param>
		void SaveOrUpdate<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>
		/// This will remove an instance of an entity from the peristance store.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		void Delete<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>
		/// This will create a query criteria for an entity or child entities for retrieving data.
		/// </summary>
		/// <typeparam name="T">Entity to query over and return results for.</typeparam>
		/// <returns></returns>
		IQuery<T> CreateQueryFor<T>() where T : class, new();

		/// <summary>
		/// This will create a simple query criteria via LINQ for an entity for retrieving data w/o JOIN support.
		/// </summary>
		/// <typeparam name="T">Entity to query over and return results for.</typeparam>
		/// <returns></returns>
		IQueryContext<T> QueryOver<T>() where T : class, new();

		/// <summary>
		/// This will allow for the execution of a stored procedure against the persistence store.
		/// </summary>
		/// <returns></returns>
		IQueryByStoredProcedure ExecuteProcedure();

		/// <summary>
		/// This will create a transaction for interacting with the local persistance store.
		/// </summary>
		/// <returns></returns>
		ITransaction BeginTransaction();
	}
}