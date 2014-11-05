using System;

namespace MicroORM.Configuration.Impl
{
    /// <summary>
    /// Instance of a named session container connection registration.
    /// </summary>
    internal sealed class SessionContainerRegistration
    {
        public string Connection { get; private set; }
        public Type Contract { get; private set; }
        public Type Service { get; private set; }

        public SessionContainerRegistration(string connection, Type contract, Type service)
        {
            Connection = connection;
            Contract = contract;
            Service = service;
        }
    }
}