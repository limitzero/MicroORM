using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.Mapping
{
	public interface IEntityMap
	{
		TableInfo Build(IMetadataStore metadataStore);
	}
}