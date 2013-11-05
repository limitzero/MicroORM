using System;
using System.Data.SqlClient;
using System.Linq;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM
{
	public class SessionFactory : ISessionFactory
	{
		private readonly IMetadataStore metadataStore;

		public SessionFactory(IMetadataStore metadataStore)
		{
			if (metadataStore != null)
			{
				this.metadataStore = metadataStore;
			}
		}

		public ISession OpenSession()
		{
			var connectionString = MicroORM.Configuration.Instance.ConnectionProvider.GetConnectionString();
			var connection = new SqlConnection(connectionString);
			return new Session(connection, this.metadataStore);
		}

		public ISession OpenSession(string connection)
		{
			var cxn = new SqlConnection(connection);
			return new Session(cxn, this.metadataStore);
		}

		public ISession OpenSessionViaAlias(string alias)
		{
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
		}
	}
}