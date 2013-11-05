using System.Collections.Generic;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.GroupBy
{
	/// <summary>
	/// This will allow the grouping by a field that was aliased in the select statement
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class AliasFieldFromEntityGroupOption<TEntity> : IGroupByOption
	{
		private readonly string field;

		public ICollection<string> Fields { get; set; }
		public IMetadataStore MetadataStore { get; set; }

		public AliasFieldFromEntityGroupOption(string field)
		{
			this.Fields = new List<string>();
			this.field = field;
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				string targetField = field;
				this.Fields = new List<string> {targetField};
			}
		}
	}
}