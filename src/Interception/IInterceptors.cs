using System.Collections.Generic;

namespace MicroORM.Interception
{
	/// <summary>
	/// Basic contract for data interceptor
	/// </summary>
	public interface IInterceptor
	{
	}

	/// <summary>
	/// Basic contract for intercepting insert events during persistance.
	/// </summary>
	public interface IInsertInterceptor : IInterceptor
	{
		/// <summary>
		/// This will allow for modifications to the enity before 
		/// the call to the insert is done. In order to invoke the 
		/// default persistance for the OnPreXXX action, 
		/// be sure to call <seealso cref="IDataInvocation.Proceed"/>.
		/// </summary>
		/// <param name="invocation"></param>
		/// <returns>
		/// Boolean: 
		/// true - proceed with insert operation
		/// false - halt insert operation.
		/// </returns>
		bool OnPreInsert(IDataInvocation invocation);

		/// <summary>
		/// This will allow for modifications to the enity after 
		/// the call to the insert is done and the entity is persisted. 
		/// In this OnPostXXX method, the call to 
		/// <seealso cref="IDataInvocation.Proceed"/> will be ignored 
		/// as the indicated persistance action has taken place in the 
		/// OnPreXXX method.
		/// </summary>
		/// <param name="invocation"></param>
		void OnPostInsert(IDataInvocation invocation);
	}

	/// <summary>
	/// Basic contract for intercepting update events during persistance.
	/// </summary>
	public interface IUpdateInterceptor : IInterceptor
	{
		/// <summary>
		/// This will allow for modifications to the enity before 
		/// the call to the update is done. In order to invoke the 
		/// default persistance for the OnPreXXX action, 
		/// be sure to call <seealso cref="IDataInvocation.Proceed"/>.
		/// </summary>
		/// <param name="invocation"></param>
		/// <returns>
		/// Boolean: 
		/// true - proceed with update operation
		/// false - halt update operation.
		/// </returns>
		bool OnPreUpdate(IDataInvocation invocation);

		/// <summary>
		/// This will allow for modifications to the enity after 
		/// the call to the update is done and the entity is persisted. 
		/// In this OnPostXXX method, the call to 
		/// <seealso cref="IDataInvocation.Proceed"/> will be ignored 
		/// as the indicated persistance action has taken place in the 
		/// OnPreXXX method.
		/// </summary>
		/// <param name="invocation"></param>
		void OnPostUpdate(IDataInvocation invocation);
	}

	/// <summary>
	/// Basic contract for intercepting delete events during persistance.
	/// </summary>
	public interface IDeleteInterceptor : IInterceptor
	{
		/// <summary>
		/// This will allow for modifications to the enity before 
		/// the call to the delete is done. In order to invoke the 
		/// default persistance for the OnPreXXX action, 
		/// be sure to call <seealso cref="IDataInvocation.Proceed"/>.
		/// </summary>
		/// <param name="invocation"></param>
		/// <returns>
		/// Boolean: 
		/// true - proceed with delete operation
		/// false - halt delete operation.
		/// </returns>
		bool OnPreDelete(IDataInvocation invocation);

		/// <summary>
		/// This will allow for modifications to the enity after 
		/// the call to the delete is done and the entity is persisted and/or removed. 
		/// In this OnPostXXX method, the call to 
		/// <seealso cref="IDataInvocation.Proceed"/> will be ignored 
		/// as the indicated persistance action has taken place in the 
		/// OnPreXXX method.
		/// </summary>
		/// <param name="invocation"></param>
		void OnPostDelete(IDataInvocation invocation);
	}
}