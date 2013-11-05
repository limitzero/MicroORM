namespace MicroORM.Interception
{
	public interface IInterceptorPipeline
	{
		void ExecuteOnInsert(IDataInvocation invocation);
		void ExecuteOnUpdate(IDataInvocation invocation);
		void ExecuteOnDelete(IDataInvocation invocation);
	}
}