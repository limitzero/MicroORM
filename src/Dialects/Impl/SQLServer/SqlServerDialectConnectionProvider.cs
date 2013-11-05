using System.Data.SqlClient;

namespace MicroORM.Dialects.Impl.SQLServer
{
	public class SQLServerDialectConnectionProvider : IDialectConnectionProvider
	{
		public string Server { get; set; }
		public string Catalog { get; set; }
		public string User { get; set; }
		public string Password { get; set; }

		public SQLServerDialectConnectionProvider(string server, string catalog)
			: this(server, catalog, string.Empty, string.Empty)
		{
		}

		public SQLServerDialectConnectionProvider(string server, string catalog, string user, string password)
		{
			Server = server;
			Catalog = catalog;
			User = user;
			Password = password;
		}

		public string GetConnectionString()
		{
			var builder = new SqlConnectionStringBuilder();
			builder.DataSource = this.Server;
			builder.InitialCatalog = this.Catalog;

			if (string.IsNullOrEmpty(this.User) == false && string.IsNullOrEmpty(this.Password))
			{
				builder.UserID = this.User;
				builder.Password = this.Password;
			}
			else
			{
				builder.IntegratedSecurity = true;
			}

			return builder.ToString();
		}
	}
}