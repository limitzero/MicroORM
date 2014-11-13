using System;
using System.Collections.Generic;
using System.Linq;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;

namespace MicroORM.Interception.Impl
{
    public class InterceptorPipeline : IInterceptorPipeline
    {
        private readonly IEnvironmentSettings _environment;

        public InterceptorPipeline(IEnvironmentSettings environment)
        {
            _environment = environment;
        }

        public void ExecuteOnInsert(IDataInvocation invocation)
        {
            var canProceed = true;
            var insertInterceptors = ResolveForInterceptor<IInsertInterceptor>();

            if ( insertInterceptors != null && insertInterceptors.Count() > 0 )
            {
                // pre-insert interceptors:
                foreach ( var interceptor in insertInterceptors )
                {
#if DEBUG
                    Type theEntity = invocation.Entity.GetType().IsProxy()
                                        ? invocation.Entity.GetType().BaseType
                                        : invocation.Entity.GetType();

                    _environment.Logger.DebugFormat("Intercepting insert for '{0}' " +
                                                                     "with interceptor '{1}' for OnPreInsert(...).",
                                                                     theEntity.FullName,
                                                                     interceptor.GetType().Name);
#endif

                    canProceed = interceptor.OnPreInsert(invocation);
                    if ( !canProceed )
                        break;
                }

                if ( canProceed )
                {
                    // do the insert operation:
                    ( (DataInvocation)invocation ).Proceed();

                    // post-insert interceptors:
                    foreach ( var interceptor in insertInterceptors )
                    {
#if DEBUG
                        Type theEntity = invocation.Entity.GetType().IsProxy()
                                            ? invocation.Entity.GetType().BaseType
                                            : invocation.Entity.GetType();

                        _environment.Logger.DebugFormat("Intercepting insert for '{0}' " +
                                                                     "with interceptor '{1}' for OnPostInsert(...).",
                                                                     theEntity.FullName,
                                                                     interceptor.GetType().Name);
#endif
                        interceptor.OnPostInsert(invocation);
                    }
                }
            }
            else
            {
                ( (DataInvocation)invocation ).Proceed();
            }
        }

        public void ExecuteOnUpdate(IDataInvocation invocation)
        {
            var canProceed = true;
            var updateInterceptors = ResolveForInterceptor<IUpdateInterceptor>();

            if ( updateInterceptors != null && updateInterceptors.Count() > 0 )
            {
                // pre-update interceptors:
                foreach ( var interceptor in updateInterceptors )
                {
#if DEBUG
                    Type theEntity = invocation.Entity.GetType().IsProxy()
                                        ? invocation.Entity.GetType().BaseType
                                        : invocation.Entity.GetType();

                    _environment.Logger.DebugFormat("Intercepting update for '{0}' " +
                                                                 "with interceptor '{1}' for OnPreUpdate(...).",
                                                                 theEntity.FullName,
                                                                 interceptor.GetType().Name);
#endif
                    canProceed = interceptor.OnPreUpdate(invocation);
                    if ( !canProceed )
                        break;
                }

                if ( canProceed )
                {
                    // do the update operation:
                    ( (DataInvocation)invocation ).Proceed();

                    // post-update interceptors:
                    foreach ( var interceptor in updateInterceptors )
                    {
#if DEBUG
                        Type theEntity = invocation.Entity.GetType().IsProxy()
                                            ? invocation.Entity.GetType().BaseType
                                            : invocation.Entity.GetType();

                       	_environment.Logger.DebugFormat("Intercepting update for '{0}' " +
                                                                         "with interceptor '{1}' for OnPostUpdate(...).",
                                                                         theEntity.FullName,
                                                                         interceptor.GetType().Name);
#endif
                        interceptor.OnPostUpdate(invocation);
                    }
                }
            }
            else
            {
                ( (DataInvocation)invocation ).Proceed();
            }
        }

        public void ExecuteOnDelete(IDataInvocation invocation)
        {
            var canProceed = true;
            var deleteInterceptors = ResolveForInterceptor<IDeleteInterceptor>();

            if ( deleteInterceptors != null && deleteInterceptors.Count() > 0 )
            {
                // pre-delete interceptors:
                foreach ( var interceptor in deleteInterceptors )
                {
#if DEBUG
                    Type theEntity = invocation.Entity.GetType().IsProxy()
                                        ? invocation.Entity.GetType().BaseType
                                        : invocation.Entity.GetType();

                  	_environment.Logger.DebugFormat("Intercepting delete for '{0}' " +
                                                                     "with interceptor '{1}' for OnPreDelete(...).",
                                                                     theEntity.FullName,
                                                                     interceptor.GetType().Name);
#endif
                    canProceed = interceptor.OnPreDelete(invocation);
                    if ( !canProceed )
                        break;
                }

                if ( canProceed )
                {
                    // do the delete operation:
                    ( (DataInvocation)invocation ).Proceed();

                    // post-delete interceptors:
                    foreach ( var interceptor in deleteInterceptors )
                    {
#if DEBUG
                        Type theEntity = invocation.Entity.GetType().IsProxy()
                                            ? invocation.Entity.GetType().BaseType
                                            : invocation.Entity.GetType();

                       	_environment.Logger.DebugFormat("Intercepting delete for '{0}' " +
                                                                         "with interceptor '{1}' for OnPostDelete(...).",
                                                                         theEntity.FullName,
                                                                         interceptor.GetType().Name);
#endif
                        interceptor.OnPostDelete(invocation);
                    }
                }
            }
            else
            {
                ( (DataInvocation)invocation ).Proceed();
            }
        }

        private IEnumerable<T> ResolveForInterceptor<T>()
        {
            return ( from match in this._environment.Interceptors
                     where typeof(T).IsAssignableFrom(match.GetType())
                     select (T)Activator.CreateInstance(match.GetType()) )
                .ToList().Distinct();

            //return ( from match in MicroORM.Configuration.Impl.Configuration.Instance.Interceptors
            //         where typeof(T).IsAssignableFrom(match.GetType())
            //         select (T)Activator.CreateInstance(match.GetType()) )
            //    .ToList().Distinct();
        }
    }
}