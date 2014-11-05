using System;

namespace MicroORM.Configuration
{
    /// <summary>
    /// Contract for a container to create named session instances for multiple database access.
    /// </summary>
    public interface ISessionContainer : IDisposable
    {
        /// <summary>
        /// This will register the contract and service instance for the container to host 
        /// the named connection to the datastore.
        /// </summary>
        /// <typeparam name="TContract">Contract/Interface defining the named session container <seealso cref="INamedSession"/></typeparam>
        /// <typeparam name="TService">Concrete instance hosting the session to the data store</typeparam>
        /// <param name="connection">Connection string to the data store</param>
        /// <returns></returns>
        ISessionContainer Register<TContract, TService>(string connection)
            where TContract : INamedSession
            where TService : class, INamedSession, new();

        /// <summary>
        /// This will create an instance of the name session container with the session connection 
        /// information set for access to the datastore.
        /// </summary>
        /// <typeparam name="T">Contract/Interface defining the named session container <seealso cref="INamedSession"/></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : INamedSession;
    }
}