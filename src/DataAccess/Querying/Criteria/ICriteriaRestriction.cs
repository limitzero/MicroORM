using System.Collections.Generic;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.DataAccess.Querying.Criteria
{
	public interface ICriteriaRestriction
	{
		ICollection<string> Expressions { get; set; }
		IMetadataStore MetadataStore { get; set; }
		QueryParameter Parameter { get; set; }
		void Build();
	}
}