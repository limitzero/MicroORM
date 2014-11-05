using System;
using System.Reflection;

namespace MicroORM.DataAccess.Internals.Impl
{
    public class PrimaryKeyInfo : ColumnInfo
    {
        public string PrimaryKeyName { get; private set; }

        public PrimaryKeyInfo(Type entity, PropertyInfo column, string dataColumnName)
            : base(entity, column, dataColumnName)
        {
        }

        public PrimaryKeyInfo(IMetadataStore metadataStore, Type entity, PropertyInfo column)
            : base(metadataStore, entity, column)
        {
        }

        public override void Initialize()
        {
            var attrs = this.Column.GetCustomAttributes(typeof(PrimaryKeyAttribute), false);

            if ( attrs.Length > 0 )
            {
                this.DataColumnName = ( (PrimaryKeyAttribute)attrs[0] ).Name;
            }

            base.Initialize();
        }

        public string GetPrimaryKeyName()
        {
            var primaryKey = string.IsNullOrEmpty(this.PrimaryKeyName)
                    ? this.DataColumnName
                    : this.PrimaryKeyName;
            return primaryKey;
        }
    }
}