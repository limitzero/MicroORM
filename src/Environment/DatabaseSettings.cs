namespace MicroORM.Environment
{
	public class DatabaseSettings
	{
		public string Alias { get; private set; }
		public string Server { get; private set; }
		public string Database { get; private set; }
		public string UserName { get; private set; }
		public string Password { get; private set; }

		public DatabaseSettings(string @alias, string server, string database, string userName, string password)
		{
			Alias = alias;
			Server = server;
			Database = database;
			UserName = userName;
			Password = password;
		}
	}
}