using System.Collections.Generic;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Selects.Impl
{
	public class AllFromEntitySelectOption<TEntity> : ISelectOption
	{
		public IMetadataStore MetadataStore { get; set; }
		public ICollection<string> Fields { get; set; }
		public TEntity Entity { get; set; }

		public AllFromEntitySelectOption()
		{
			this.Fields = new List<string>();
		}

		public void Build()
		{
			if (this.MetadataStore.Entities.ContainsKey(typeof (TEntity)))
			{
				var table = this.MetadataStore.GetTableInfo<TEntity>();
				string fieldformat = "[{0}].[{1}]";

				foreach (var field in table.GetFieldsForSelect())
				{
					this.Fields.Add(string.Format(fieldformat, table.TableName, field));
				}
			}
		}
	}
}