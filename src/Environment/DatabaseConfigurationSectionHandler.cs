using System.Configuration;

namespace MicroORM.Environment
{
	public class DatabaseConfigurationSectionHandler : ConfigurationSection
	{
		public static readonly string SectionName = "micro.orm";

		[ConfigurationProperty("aliases")]
		public AliasElementCollection AliasElements
		{
			get { return ((AliasElementCollection) (base["aliases"])); }
			set { base["aliases"] = value; }
		}
	}
}