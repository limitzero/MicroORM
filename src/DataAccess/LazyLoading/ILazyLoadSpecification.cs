using System.Collections.Generic;

namespace MicroORM.DataAccess.LazyLoading
{
	public interface ILazyLoadSpecification
	{
		void Initialize(IEnumerable<string> properties);
		bool IsMetBy(string property);
		bool IsLazyLoadingEnabled { get; set; }
		void MarkAsLazyLoaded(string property);
	}
}