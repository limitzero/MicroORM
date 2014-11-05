using System;
using System.Data;
using System.Reflection;

namespace MicroORM.DataAccess.Internals.Impl
{
	public class ColumnInfo
	{
		public string DataColumnName { get; protected set; }
		public IMetadataStore MetadataStore { get; set; }
		public Type Entity { get; private set; }
		public PropertyInfo Column { get; protected set; }
		public DbType DbType { get; protected set; }
		public bool IsNullable { get; private set; }
        public bool IsLazyLoaded { get; set; }

		public ColumnInfo(Type entity, PropertyInfo column, string dataColumnName)
		{
			Entity = entity;
			Column = column;
			DataColumnName = dataColumnName;
			LoadColumn();
		}

		public ColumnInfo(IMetadataStore metadataStore, Type entity, PropertyInfo column)
		{
			MetadataStore = metadataStore;
			Entity = entity;
			Column = column;
			LoadColumn();
		}

		public virtual void Initialize()
		{
		}

		private void LoadColumn()
		{
			if (this.Column != null)
			{
				this.IsNullable = this.Column.PropertyType.FullName.Contains("Nullable");

				if (this.MetadataStore != null)
				{
					this.DataColumnName = this.MetadataStore.GetColumnNameFromEntityProperty(this.Column);
				}

				if (this.Column.PropertyType.FullName.StartsWith("System") == true &&
				    this.Column.PropertyType.IsGenericType == false)
				{
					this.DbType = TypeConverter.ConvertToDbType(this.Column.PropertyType);
				}

				this.Initialize();
			}
		}
	}
}