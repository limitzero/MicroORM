using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Castle.Core;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.Configuration.Impl
{
    /// <summary>
    /// Concrete instance of a container to host factories to yield named instances of 
    /// sessions for multiple database access.
    /// </summary>
    public sealed class NamedSessionContainer : INamedSessionContainer
    {
        private readonly IEnvironmentSettings _environment;

        private ConcurrentDictionary<string, ISessionFactory> _sessionFactories = 
            new ConcurrentDictionary<string, ISessionFactory>();

        private ConcurrentBag<NamedSessionContainerRegistration> _registrations =
            new ConcurrentBag<NamedSessionContainerRegistration>();

        private bool _disposed;

        public NamedSessionContainer(IEnvironmentSettings environment = null)
        {
            _environment = environment;
        }

        public INamedSessionContainer Register<TContract, TService>(string connection)
            where TContract : INamedSession
            where TService : class, INamedSession, new()
        {
            Register<TContract, TService>(connection, null);
            return this;
        }

        public INamedSessionContainer Register<TContract, TService>(string connection, params Assembly[] assemblies)
            where TContract : INamedSession
            where TService : class, INamedSession, new()
        {
            if ( _disposed )
                return this;

            var registration = 
                new NamedSessionContainerRegistration(connection, typeof (TContract), typeof (TService))
            {
                Assemblies = assemblies
            };

            if ( _registrations.Contains(registration) )
                return this;

            _registrations.Add(registration);

            return this;
        }

        public T Resolve<T>() where T : INamedSession
        {
            var result = default(T);
            ISessionFactory factory = null; 

            if ( _disposed )
                return result;

            var registration = _registrations.FirstOrDefault(r => r.Contract == typeof(T));

            if ( registration == null )
                return result;

            if (_sessionFactories.ContainsKey(registration.Connection) == false)
            {
                factory = new SessionFactory(new MetadataStore());
                _sessionFactories.TryAdd(registration.Connection, factory);
            }
            else
            {
                factory = _sessionFactories[registration.Connection];
            }

            var session = factory.OpenSession(registration.Connection);

            var created = Activator.CreateInstance(registration.Service);
            var instance = (T)created;

            instance.Session = session;

            return instance;
        }

        ~NamedSessionContainer()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if ( _disposed )
                return;

            if ( disposing )
            {
                if ( _sessionFactories != null )
                {
                    _sessionFactories.ForEach(kvp => kvp.Value.Dispose());
                    _sessionFactories.Clear();
                }
                _sessionFactories = null;

                _registrations = null;
            }

            _disposed = true;
        }
    }

}