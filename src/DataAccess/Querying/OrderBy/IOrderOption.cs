using System.Collections.Generic;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.OrderBy
{
	public interface IOrderOption
	{
		ICollection<string> Fields { get; set; }
		IMetadataStore MetadataStore { get; set; }
		void Build();
	}

	public interface IOrderOption<TEntity> : IOrderOption
	{
	}
}