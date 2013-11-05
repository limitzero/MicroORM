using System.Collections.Generic;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Selects
{
	public interface ISelectOption
	{
		IMetadataStore MetadataStore { get; set; }
		ICollection<string> Fields { get; set; }
		void Build();
	}
}