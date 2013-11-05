using System.Collections.Generic;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.GroupBy
{
	public interface IGroupByOption
	{
		IMetadataStore MetadataStore { get; set; }
		ICollection<string> Fields { get; set; }
		void Build();
	}
}