using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Core.Interceptor;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.LazyLoading
{
	internal class LazyLoadingInterceptor : Castle.Core.Interceptor.IInterceptor
	{
		private readonly Session session;
		private readonly TableInfo tableInfo;

		public LazyLoadingInterceptor(Session session, TableInfo tableInfo)
		{
			this.session = session;
			this.tableInfo = tableInfo;
		}

		public void Intercept(IInvocation invocation)
		{
			ILazyLoadSpecification lazyLoadSpecification =
				invocation.InvocationTarget as ILazyLoadSpecification;

			// only for "getters" will we do lazy loading (with exception of mixed-in properties):
			if (invocation.Method.Name.StartsWith("get_")
			    && invocation.Method.Name.Contains("IsLazyLoadingEnabled") == false)
			{
				var property = invocation.Method.Name.Replace("get_", string.Empty);

				if (lazyLoadSpecification != null)
				{
					// lazily load the entity property only once in the session:
					if (lazyLoadSpecification.IsLazyLoadingEnabled
					    && lazyLoadSpecification.IsMetBy(property))
					{
						var createdTypeToLoad = CreateTypeToDynamicallyLoad(invocation);
						session.InitializeProxy(property, invocation.Proxy, createdTypeToLoad);
						lazyLoadSpecification.MarkAsLazyLoaded(property);
					}
				}
			}

			invocation.Proceed();
		}

		private static Type CreateTypeToDynamicallyLoad(IInvocation invocation)
		{
			Type theTypeToLoadDynamically = null;

			if (typeof (IEnumerable).IsAssignableFrom(invocation.Method.ReturnType))
			{
				var type = invocation.Method.ReturnType.GetGenericArguments()[0];
				theTypeToLoadDynamically = typeof (List<>).MakeGenericType(type);
			}
			else
			{
				theTypeToLoadDynamically = invocation.Method.ReturnType;
			}

			return theTypeToLoadDynamically;
		}
	}
}