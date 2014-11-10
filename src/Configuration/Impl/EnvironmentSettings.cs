using System.Collections.Generic;
using MicroORM.Interception;
using MicroORM.Logging;

namespace MicroORM.Configuration.Impl
{
    internal class EnvironmentSettings : IEnvironmentSettings
    {
        public HashSet<IInterceptor> Interceptors { get; private set; }
        public ILogger Logger { get; private set; }

        public EnvironmentSettings(
            IEnumerable<IInterceptor> interceptors, 
            ILogger logger)
        {
            this.Interceptors = new HashSet<IInterceptor>(interceptors);
            this.Logger = logger; 

            if(Logger == null)
                this.Logger = new ConsoleLogger(); 
        }
    }
}