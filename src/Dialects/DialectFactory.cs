using System.Linq;
using System.Reflection;
using MicroORM.Dialects.Impl.SQLServer;

namespace MicroORM.Dialects
{
    internal class DialectFactory : IDialectFactory
    {
        public IDialect Create(string connectionString)
        {
            var dialect = SearchForDialect(connectionString);

            if ( dialect == null )
                dialect = new SqlServerDialect();

            return dialect;
        }

        private IDialect CreateDialectForSqlServer(string connectionString)
        {
            IDialect dialect = null;

            if ( connectionString.Contains("Initial Catalog") )
                dialect = new SqlServerDialect();

            return dialect;
        }

        private IDialect SearchForDialect(string connectionString)
        {
            IDialect foundDialect = null;

            var dialects = this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("CreateDialect"))
                .Select(m => m).ToList();

            if ( dialects.Any() == false )
                return foundDialect;

            foreach ( var dialect in dialects )
            {
                try
                {
                    var result = dialect.Invoke(this, new object[] { connectionString })
                        as IDialect;

                    if ( result != null )
                    {
                        foundDialect = result;
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return foundDialect;
        }
    }
}