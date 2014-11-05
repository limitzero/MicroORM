using System;
using System.Collections.Concurrent;
using System.Linq;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Dialects;

namespace MicroORM
{
	internal class SessionFactory : ISessionFactory
	{
		private readonly IMetadataStore metadataStore;
	    private bool _disposed;
        private ConcurrentBag<ISession> _sessions;

		public SessionFactory(IMetadataStore metadataStore)
		{
			if (metadataStore != null)
			{
				this.metadataStore = metadataStore;
			}

            _sessions = new ConcurrentBag<ISession>();
		}

		public ISession OpenSession()
		{
            throw new NotImplementedException();
            //var connectionString = MicroORM.Configuration.Instance.ConnectionProvider.GetConnectionString();
            //var connection = new SqlConnection(connectionString);
            //return new Session(connection, this.metadataStore);
		}

		public ISession OpenSession(string connectionString)
		{
		    var dialectFactory = new DialectFactory();
		    var dialect = dialectFactory.Create(connectionString);

		    var connection = dialect.CreateConnection(connectionString);

            return new Session(connection, this.metadataStore, dialect);
		}

		public ISession OpenSessionViaAlias(string alias)
		{
            throw new NotImplementedException();

            /*
			var databaseAlias = (from item in MicroORM.Configuration.Instance.ExternalDatabaseSettings
			                     where item.Alias.ToLower().Trim().Equals(alias.ToLower().Trim())
			                     select item).FirstOrDefault();

			if (databaseAlias == null)
				throw new ArgumentException(
					string.Format(
						"No alias under the name '{0}' could be found in the external configuration for connecting to the database. Please check your configuration and re-try the operation.",
						alias));

			// use sql builder to build the connection and pass it to the session:
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();


			if (string.IsNullOrEmpty(databaseAlias.UserName) == false
			    && string.IsNullOrEmpty(databaseAlias.Password) == false)
			{
				builder.UserID = databaseAlias.UserName;
				builder.Password = databaseAlias.Password;
			}
			else
			{
				builder.IntegratedSecurity = true;
			}

			builder.DataSource = databaseAlias.Server;
			builder.InitialCatalog = databaseAlias.Database;

			var cxn = new SqlConnection(builder.ConnectionString);
			return new Session(cxn, this.metadataStore);
             */
		}

	    public void Dispose()
	    {
            Dispose(true);
            GC.SuppressFinalize(this);
	    }

	    protected virtual void Dispose(bool disposing)
	    {
	        if (_disposed) return;

	        if (disposing)
	        {
	            if (this._sessions.Any())
	            {
	                foreach (var session in _sessions)
	                {
	                    try
	                    {
	                        DisposeOfSession(session);
	                    }
	                    catch
	                    {
	                        // ok..connection should be closed by data provider after execution
	                    }
	                }
	                this._sessions = null;
	            }

	            _disposed = true;
	        }
	    }

        private void DisposeOfSession(ISession session)
        {
            if ( session != null )
                session.Dispose();
        }
	}
}