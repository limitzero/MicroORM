using System;
using System.Configuration;

namespace MicroORM.Environment
{
	[ConfigurationCollection(typeof (AliasElement))]
	public class AliasElementCollection : ConfigurationElementCollection
	{
		internal const string PropertyName = "Alias";

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMapAlternate; }
		}

		protected override string ElementName
		{
			get { return PropertyName; }
		}

		protected override bool IsElementName(string elementName)
		{
			return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new AliasElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((AliasElement) (element)).Name;
		}

		public AliasElement this[int idx]
		{
			get { return (AliasElement) BaseGet(idx); }
		}
	}
}