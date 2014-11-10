using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroORM.Configuration.Impl
{
    /// <summary>
    /// Instance of a named session container connection registration.
    /// </summary>
    internal sealed class NamedSessionContainerRegistration
    {
        public string Connection { get; private set; }
        public Type Contract { get; private set; }
        public Type Service { get; private set; }
        public  IList<Assembly> Assemblies { get; set; }

        public NamedSessionContainerRegistration(string connection, Type contract, Type service)
        {
            Connection = connection;
            Contract = contract;
            Service = service;
        }
    }
}