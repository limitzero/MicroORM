using System;
using System.Configuration;

namespace MicroORM.Environment
{
	public class AliasElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (String) this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("server", IsRequired = true)]
		public string Server
		{
			get { return (String) this["server"]; }
			set { this["server"] = value; }
		}

		[ConfigurationProperty("database", IsRequired = true)]
		public string Database
		{
			get { return (String) this["database"]; }
			set { this["database"] = value; }
		}

		[ConfigurationProperty("username", DefaultValue = "")]
		public string UserName
		{
			get { return (String) this["username"]; }
			set { this["username"] = value; }
		}

		[ConfigurationProperty("password", DefaultValue = "")]
		public string Password
		{
			get { return (String) this["password"]; }
			set { this["password"] = value; }
		}
	}
}