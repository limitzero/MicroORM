using System;
using System.Reflection;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.Querying.Impl
{
	public class QueryParameter
	{
		public Type Entity { get; private set; }
		public ColumnInfo Property { get; private set; }
		public object Value { get; private set; }

		public QueryParameter(Type entity, ColumnInfo property, object value, object rawValue)
		{
			Entity = entity;
			Property = property;
			Value = value;

			if (rawValue.GetType() != property.Column.PropertyType)
				throw new InvalidCastException(string.Format("Type Check Violation: For the property '{0}' on entity '{1}' " +
				                                             "the query parameter input of '{2}' with type '{3}' does not match " +
				                                             "the property type of '{4}' " +
				                                             "for the selected property on the entity.",
				                                             property.Column.Name,
				                                             entity.FullName,
				                                             rawValue,
				                                             rawValue.GetType().FullName,
				                                             property.Column.PropertyType.FullName));
		}
	}
}