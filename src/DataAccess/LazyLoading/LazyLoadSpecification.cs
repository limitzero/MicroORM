using System.Collections.Generic;

namespace MicroORM.DataAccess.LazyLoading
{
	public class LazyLoadSpecification : ILazyLoadSpecification
	{
		private readonly List<string> properties;
		private IDictionary<string, bool> lazyLoadedProperties;

		public bool IsLazyLoadingEnabled { get; set; }

		public LazyLoadSpecification()
		{
			this.properties = new List<string>();
			this.lazyLoadedProperties = new Dictionary<string, bool>();
		}

		public void Initialize(IEnumerable<string> properties)
		{
			this.properties.AddRange(properties);
		}

		public bool IsMetBy(string property)
		{
			bool hasBeenLazyLoaded = false;
			this.lazyLoadedProperties.TryGetValue(property, out hasBeenLazyLoaded);

			return this.properties.Contains(property) && hasBeenLazyLoaded == false;
		}

		public void MarkAsLazyLoaded(string property)
		{
			if (this.properties.Contains(property))
			{
				if (this.lazyLoadedProperties.ContainsKey(property) == false)
					this.lazyLoadedProperties.Add(property, true);
			}
		}
	}
}