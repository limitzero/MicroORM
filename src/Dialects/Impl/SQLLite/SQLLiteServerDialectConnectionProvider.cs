namespace MicroORM.Dialects.Impl.SQLLite
{
	public class SQLLiteServerDialectConnectionProvider : IDialectConnectionProvider
	{
		public string Database { get; set; }
		public string Password { get; set; }

		public SQLLiteServerDialectConnectionProvider(string database)
			: this(database, string.Empty)
		{
		}

		public SQLLiteServerDialectConnectionProvider(string database, string password)
		{
			Database = database;
			Password = password;
		}

		public string GetConnectionString()
		{
			string connectionString = string.Empty;
			string connectionWithPassword = "Data Source = {0}; Version = 3; Password = {1}";
			//string connectionWithOutPassword = "Data Source = {0}; Version = 3;";
			string connectionWithOutPassword = "Data Source = {0};";

			if (string.IsNullOrEmpty(this.Password) == true)
			{
				connectionString = string.Format(connectionWithOutPassword, this.Database);
			}
			else
			{
				connectionString = string.Format(connectionWithPassword, this.Database, this.Password);
			}

			return connectionString;
		}
	}
}