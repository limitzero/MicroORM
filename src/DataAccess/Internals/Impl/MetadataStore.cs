using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroORM.DataAccess.Extensions;

namespace MicroORM.DataAccess.Internals.Impl
{
	public class MetadataStore : IMetadataStore
	{
		private readonly IDictionary<Type, TableInfo> _tableInfoCache;

		public IDictionary<Type, List<PropertyInfo>> Entities { get; private set; }

		public MetadataStore()
		{
			this._tableInfoCache = new Dictionary<Type, TableInfo>();
			this.Entities = new Dictionary<Type, List<PropertyInfo>>();
		}

		public string GetTableName(Type type)
		{
			string table = type.Name;

			if (this.Entities.ContainsKey(type))
			{
				var attr = GetAttribute<TableAttribute>(type);

				if (attr != null)
				{
					table = attr.Name;
				}
			}

			return table;
		}

		public string GetTablePrimaryKeyName(Type type)
		{
			string primaryKey = string.Empty;

			if (this.Entities.ContainsKey(type))
			{
				var property = (from match in type.GetProperties()
				                where match.GetCustomAttributes(typeof (PrimaryKeyAttribute), true).Length > 0
				                select match).FirstOrDefault();

				if (property != null)
				{
					var attrs = property.GetCustomAttributes(typeof (PrimaryKeyAttribute), false);

					if (attrs.Length > 0)
					{
						primaryKey = ((PrimaryKeyAttribute) attrs[0]).Name;
					}
					else
					{
						primaryKey = property.Name;
					}
				}
			}

			if (primaryKey == null)
			{
				var tableInfo = this.GetTableInfo(type);
				primaryKey = tableInfo.PrimaryKey.Column.Name;
			}


			return primaryKey;
		}

		public TableInfo GetTableInfo<TEntity>(Type child)
		{
			return this.GetTableInfo(typeof (TEntity), child);
		}

		public TableInfo GetTableInfo(Type entity, Type child)
		{
			TableInfo tableInfo;

			Type theType = entity;

			if (entity.IsProxy()) theType = entity.BaseType;

			if (this._tableInfoCache.TryGetValue(child, out tableInfo))
				return tableInfo;

			tableInfo = new TableInfo(child, theType, this);

			this._tableInfoCache.Add(child, tableInfo);

			return tableInfo;
		}

		public TableInfo GetTableInfo<TEntity>()
		{
			return this.GetTableInfo(typeof (TEntity));
		}

		public TableInfo GetTableInfo(Type entity)
		{
			TableInfo tableInfo;

			Type theType = entity;

			if (entity.IsProxy()) theType = entity.BaseType;

			if (this._tableInfoCache.TryGetValue(theType, out tableInfo))
				return tableInfo;

			tableInfo = new TableInfo(theType, this);

			this._tableInfoCache.Add(theType, tableInfo);

			return tableInfo;
		}

		public void SetTableInfo(TableInfo tableInfo)
		{
			if (this.Entities.ContainsKey(tableInfo.Entity) == false)
				this.Entities.Add(tableInfo.Entity, new List<PropertyInfo>());

			if (this._tableInfoCache.ContainsKey(tableInfo.Entity) == false)
				this._tableInfoCache.Add(tableInfo.Entity, tableInfo);
		}

		public void AddEntity(Type entity)
		{
			if (this.Entities.ContainsKey(entity)) return;
			Define(entity);
		}

		public string GetColumnName(Type entity, PropertyInfo property)
		{
			var name = property.Name;
			var properties = this.Entities[entity];
			var toFind = properties.Find(p => p.PropertyType == property.PropertyType &&
			                                  p.Name == property.Name);

			if (toFind != null)
			{
				ColumnAttribute columnAttribute = GetAttribute<ColumnAttribute>(toFind);
				PrimaryKeyAttribute primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(toFind);

				if (columnAttribute != null)
				{
					name = columnAttribute.Name;
				}
				else if (primaryKeyAttribute != null)
				{
					name = primaryKeyAttribute.Name;
				}
			}

			return name;
		}

		public string GetColumnName(Type entity, string propertyName)
		{
			var name = propertyName;

			var property = (from match in entity.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			                where propertyName == match.Name
			                // poor choice in comparisions...
			                select match).FirstOrDefault();

			if (property != null)
			{
				name = this.GetColumnName(entity, property);
			}

			return name;
		}

		public string GetPropertyNameFromExpression<TEntity>(Expression<Func<TEntity, object>> expression)
		{
			MemberExpression memberExpression;

			if (expression.Body is UnaryExpression)
			{
				memberExpression = ((UnaryExpression) expression.Body).Operand as MemberExpression;
			}
			else
			{
				memberExpression = expression.Body as MemberExpression;
			}

			if (memberExpression == null)
			{
				throw new InvalidOperationException("You must specify a property!");
			}

			return memberExpression.Member.Name;
		}

		public string GetColumnNameFromEntityProperty(PropertyInfo column)
		{
			string columnName = string.Empty;

			if (typeof (IEnumerable<>).IsAssignableFrom(column.PropertyType))
				return columnName;

			var attr = GetAttribute<ColumnAttribute>(column);

			if (attr != null)
				columnName = attr.Name;

			return columnName;
		}

		public PropertyInfo PrimaryKeyField(Type entity)
		{
			PropertyInfo primaryKey = null;

			if (this.Entities.ContainsKey(entity))
			{
				primaryKey = (from match in entity.GetProperties()
				              where match.GetCustomAttributes(typeof (PrimaryKeyAttribute), true).Length > 0
				              select match).FirstOrDefault();
			}

			return primaryKey;
		}

		public PropertyInfo PrimaryKeyField<TEntity>()
		{
			return this.PrimaryKeyField(typeof (TEntity));
		}

		public PropertyInfo[] Columns<TEntity>()
		{
			var columns = this.Entities[typeof (TEntity)];
			var primarykey = this.PrimaryKeyField<TEntity>();
			columns.Remove(primarykey);
			return columns.ToArray();
		}

		private void Define(Type entity)
		{
			var properties = entity.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			if (properties != null && properties.Length > 0)
				this.Entities.Add(entity, new List<PropertyInfo>(properties));
		}

		private static TAttribute GetAttribute<TAttribute>(Type type) where TAttribute : Attribute
		{
			TAttribute attr = default(TAttribute);
			var attrs = type.GetCustomAttributes(typeof (TAttribute), true);

			if (attrs.Length > 0)
			{
				attr = (TAttribute) attrs[0];
			}

			return attr;
		}

		private static TAttribute GetAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute
		{
			TAttribute attr = default(TAttribute);

			if (property == null) return attr;

			var attrs = property.GetCustomAttributes(typeof (TAttribute), true);

			if (attrs.Length > 0)
			{
				attr = (TAttribute) attrs[0];
			}

			return attr;
		}
	}
}