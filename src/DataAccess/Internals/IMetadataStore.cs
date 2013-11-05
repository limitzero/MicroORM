using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.Internals
{
	public interface IMetadataStore
	{
		IDictionary<Type, List<PropertyInfo>> Entities { get; }
		string GetTableName(Type type);
		void AddEntity(Type entity);
		string GetColumnName(Type entity, PropertyInfo property);
		string GetColumnName(Type entity, string propertyName);
		string GetPropertyNameFromExpression<TEntity>(Expression<Func<TEntity, object>> expression);
		PropertyInfo PrimaryKeyField<TEntity>();
		PropertyInfo PrimaryKeyField(Type entity);
		PropertyInfo[] Columns<TEntity>();
		string GetColumnNameFromEntityProperty(PropertyInfo column);
		string GetTablePrimaryKeyName(Type type);

		TableInfo GetTableInfo<TEntity>();
		TableInfo GetTableInfo(Type entity);
		TableInfo GetTableInfo<TEntity>(Type child);
		TableInfo GetTableInfo(Type entity, Type child);
	}
}