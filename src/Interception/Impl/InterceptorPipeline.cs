using System;
using System.Collections.Generic;
using System.Linq;
using MicroORM.DataAccess.Extensions;

namespace MicroORM.Interception.Impl
{
	public class InterceptorPipeline : IInterceptorPipeline
	{
		public void ExecuteOnInsert(IDataInvocation invocation)
		{
			var canProceed = true;
			var insertInterceptors = ResolveForInterceptor<IInsertInterceptor>();

			if (insertInterceptors != null && insertInterceptors.Count() > 0)
			{
				// pre-insert interceptors:
				foreach (var interceptor in insertInterceptors)
				{
#if DEBUG
					Type theEntity = invocation.Entity.GetType().IsProxy()
					                 	? invocation.Entity.GetType().BaseType
					                 	: invocation.Entity.GetType();
					System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting insert for '{0}' " +
					                                                 "with interceptor '{1}' for OnPreInsert(...).",
					                                                 theEntity.FullName,
					                                                 interceptor.GetType().Name));
#endif

					canProceed = interceptor.OnPreInsert(invocation);
					if (!canProceed) break;
				}

				if (canProceed)
				{
					// do the insert operation:
					((DataInvocation) invocation).Proceed();

					// post-insert interceptors:
					foreach (var interceptor in insertInterceptors)
					{
#if DEBUG
						Type theEntity = invocation.Entity.GetType().IsProxy()
						                 	? invocation.Entity.GetType().BaseType
						                 	: invocation.Entity.GetType();
						System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting insert for '{0}' " +
						                                                 "with interceptor '{1}' for OnPostInsert(...).",
						                                                 theEntity.FullName,
						                                                 interceptor.GetType().Name));
#endif
						interceptor.OnPostInsert(invocation);
					}
				}
			}
			else
			{
				((DataInvocation) invocation).Proceed();
			}
		}

		public void ExecuteOnUpdate(IDataInvocation invocation)
		{
			var canProceed = true;
			var updateInterceptors = ResolveForInterceptor<IUpdateInterceptor>();

			if (updateInterceptors != null && updateInterceptors.Count() > 0)
			{
				// pre-update interceptors:
				foreach (var interceptor in updateInterceptors)
				{
#if DEBUG
					Type theEntity = invocation.Entity.GetType().IsProxy()
					                 	? invocation.Entity.GetType().BaseType
					                 	: invocation.Entity.GetType();
					System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting update for '{0}' " +
					                                                 "with interceptor '{1}' for OnPreUpdate(...).",
					                                                 theEntity.FullName,
					                                                 interceptor.GetType().Name));
#endif
					canProceed = interceptor.OnPreUpdate(invocation);
					if (!canProceed) break;
				}

				if (canProceed)
				{
					// do the update operation:
					((DataInvocation) invocation).Proceed();

					// post-update interceptors:
					foreach (var interceptor in updateInterceptors)
					{
#if DEBUG
						Type theEntity = invocation.Entity.GetType().IsProxy()
						                 	? invocation.Entity.GetType().BaseType
						                 	: invocation.Entity.GetType();
						System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting update for '{0}' " +
						                                                 "with interceptor '{1}' for OnPostUpdate(...).",
						                                                 theEntity.FullName,
						                                                 interceptor.GetType().Name));
#endif
						interceptor.OnPostUpdate(invocation);
					}
				}
			}
			else
			{
				((DataInvocation) invocation).Proceed();
			}
		}

		public void ExecuteOnDelete(IDataInvocation invocation)
		{
			var canProceed = true;
			var deleteInterceptors = ResolveForInterceptor<IDeleteInterceptor>();

			if (deleteInterceptors != null && deleteInterceptors.Count() > 0)
			{
				// pre-delete interceptors:
				foreach (var interceptor in deleteInterceptors)
				{
#if DEBUG
					Type theEntity = invocation.Entity.GetType().IsProxy()
					                 	? invocation.Entity.GetType().BaseType
					                 	: invocation.Entity.GetType();
					System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting delete for '{0}' " +
					                                                 "with interceptor '{1}' for OnPreDelete(...).",
					                                                 theEntity.FullName,
					                                                 interceptor.GetType().Name));
#endif
					canProceed = interceptor.OnPreDelete(invocation);
					if (!canProceed) break;
				}

				if (canProceed)
				{
					// do the delete operation:
					((DataInvocation) invocation).Proceed();

					// post-delete interceptors:
					foreach (var interceptor in deleteInterceptors)
					{
#if DEBUG
						Type theEntity = invocation.Entity.GetType().IsProxy()
						                 	? invocation.Entity.GetType().BaseType
						                 	: invocation.Entity.GetType();
						System.Diagnostics.Debug.WriteLine(string.Format("MicroORM : Intercepting delete for '{0}' " +
						                                                 "with interceptor '{1}' for OnPostDelete(...).",
						                                                 theEntity.FullName,
						                                                 interceptor.GetType().Name));
#endif
						interceptor.OnPostDelete(invocation);
					}
				}
			}
			else
			{
				((DataInvocation) invocation).Proceed();
			}
		}

		private static IEnumerable<T> ResolveForInterceptor<T>()
		{
			return (from match in MicroORM.Configuration.Instance.Interceptors
			        where typeof (T).IsAssignableFrom(match.GetType())
			        select (T) Activator.CreateInstance(match.GetType()))
				.ToList().Distinct();
		}
	}
}