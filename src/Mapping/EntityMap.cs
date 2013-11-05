using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.Mapping
{
	/// <summary>
	/// Creates a mapping for a entity to a datatable.
	/// </summary>
	/// <typeparam name="T">Parent entity to be mapped</typeparam>
	public abstract class EntityMap<T> : IEntityMap where T : class
	{
		/// <summary>
		/// Gets or sets the name of the data table representing the entity.
		/// </summary>
		public string TableName { get; protected set; }

		/// <summary>
		/// Gets the column representing the primary key for the entity.
		/// </summary>
		public PrimaryKeyInfo PrimaryKey { get; private set; }

		private readonly HashSet<ColumnInfo> columns = new HashSet<ColumnInfo>();

		/// <summary>
		/// Gets the set of columns representing simple data attributes of the entity.
		/// </summary>
		public IEnumerable<ColumnInfo> Columns
		{
			get { return columns; }
		}

		private readonly HashSet<ColumnInfo> references = new HashSet<ColumnInfo>();

		/// <summary>
		/// Get the set of entities on the parent that refer to other entities with a defined lifecycle.
		/// </summary>
		public IEnumerable<ColumnInfo> References
		{
			get { return references; }
		}

		private readonly HashSet<ColumnInfo> collections = new HashSet<ColumnInfo>();

		/// <summary>
		/// Get the set of properties that are collections of child entities for parent.
		/// </summary>
		public IEnumerable<ColumnInfo> Collections
		{
			get { return collections; }
		}

		private readonly HashSet<ColumnInfo> components = new HashSet<ColumnInfo>();

		/// <summary>
		/// Gets the collection of data columns that are entities that do not have a lifecycle 
		/// but are compound attributes of a parent entity.
		/// </summary>
		public IEnumerable<ColumnInfo> Components
		{
			get { return components; }
		}

		/// <summary>
		/// This will identify the primary column on the entity for instance uniqueness.
		/// </summary>
		/// <param name="column">Expression denoting the primary key</param>
		protected void HasPrimaryKey(Expression<Func<T, object>> column)
		{
			var name = GetPropertyNameFromExpression(column);
			this.HasPrimaryKey(column, name);
		}

		/// <summary>
		/// This will identify the primary column on the entity for instance uniqueness.
		/// </summary>
		/// <param name="column">Expression denoting the primary key</param>
		/// <param name="columnName">Optional: Data column name of key (defaulted to property name)</param>
		protected void HasPrimaryKey(Expression<Func<T, object>> column, string columnName)
		{
			var name = GetPropertyNameFromExpression(column);
			var property = FindPropertyFromPropertyName(typeof (T), name);
			var columnInfo = new PrimaryKeyInfo(typeof (T), property, columnName);
			this.PrimaryKey = columnInfo;
		}

		/// <summary>
		/// This will identify a property on the entity that represents a data column for persistance
		/// (this will default the data column name to be the property name in the expression).
		/// </summary>
		/// <param name="column">Expression denoting the column</param>
		protected void HasColumn(Expression<Func<T, object>> column)
		{
			var name = GetPropertyNameFromExpression(column);
			this.HasColumn(column, name);
		}

		/// <summary>
		/// This will identify a property on the entity that represents a data column for persistance
		/// </summary>
		/// <param name="column">Expression denoting the column</param>
		/// <param name="columnName">Name of the representative data column in the persistance store.</param>
		protected void HasColumn(Expression<Func<T, object>> column, string columnName)
		{
			var name = GetPropertyNameFromExpression(column);
			var property = FindPropertyFromPropertyName(typeof (T), name);
			var columnInfo = new ColumnInfo(typeof (T), property, columnName);
			this.columns.Add(columnInfo);
		}

		/// <summary>
		/// This defines a property on an entity as having a set of child objects
		/// whose parent is this entity (defines the zero to many relationship).
		/// </summary>
		/// <param name="column"></param>
		protected void HasCollection(Expression<Func<T, IEnumerable>> column)
		{
			var name = GetPropertyNameForCollectionFromExpression(column);
			var property = FindPropertyFromPropertyName(typeof (T), name);

			var columnInfo = new ColumnInfo(typeof (T), property, string.Empty);
			this.collections.Add(columnInfo);
		}

		/// <summary>
		/// This defines a property on an entity as being a non-persistant entity 
		/// that contributes its attributes to the parent entity.
		/// </summary>
		/// <param name="column">Expression to define the property that defines the non-persistant instance.</param>
		/// <param name="withColumns">Set of defining columns <seealso cref="WithColumn"/> that will extend the primary entities list of attributes.</param>
		protected void HasComponent(Expression<Func<T, object>> column,
		                            params Tuple<Expression<Func<T, object>>, string>[] withColumns)
		{
			var componentPropertyName = GetPropertyNameFromExpression(column);
			var componentProperty = FindPropertyFromPropertyName(typeof (T), componentPropertyName);

			foreach (var withColumn in withColumns)
			{
				var dataColumnName = withColumn.Item2;
				var withColumnExpression = withColumn.Item1;

				var name = GetPropertyNameFromExpression(withColumnExpression);
				var property = FindPropertyFromPropertyName(componentProperty.PropertyType, name);

				var columnInfo = new ColumnInfo(typeof (T), componentProperty, dataColumnName);

				this.components.Add(columnInfo);
			}
		}

		/// <summary>
		/// This defines the support column definition of an instance that is used to extend a primary entity's set of attributes.
		/// </summary>
		/// <param name="column">Expression to define the column for the instance. The data store column name will be same as property name</param>
		/// <returns></returns>
		protected Tuple<Expression<Func<T, object>>, string> WithColumn(Expression<Func<T, object>> column)
		{
			var name = GetPropertyNameFromExpression(column);
			return new Tuple<Expression<Func<T, object>>, string>(column, name);
		}

		/// <summary>
		/// This defines the support column definition of an instance that is used to extend a primary entity's set of attributes.
		/// </summary>
		/// <param name="column">Expression to define the column for the instance</param>
		/// <param name="columnName">Name of the representative column in the data store.</param>
		/// <returns></returns>
		protected Tuple<Expression<Func<T, object>>, string> WithColumn(Expression<Func<T, object>> column, string columnName)
		{
			return new Tuple<Expression<Func<T, object>>, string>(column, columnName);
		}

		/// <summary>
		/// This defines a property on an entity as having a direct association to another 
		/// entity (defines many to one relationship).
		/// </summary>
		/// <param name="column"></param>
		protected void HasReference(Expression<Func<T, object>> column)
		{
			var referencePropertyName = GetPropertyNameFromExpression(column);
			var referenceProperty = FindPropertyFromPropertyName(typeof (T), referencePropertyName);

			var columnInfo = new ColumnInfo(typeof (T), referenceProperty, referencePropertyName);
			this.references.Add(columnInfo);
		}

		/// <summary>
		/// This defines the entity association that represents a join table for the many-to-many
		/// relationship between one entity and another entity via an intermediary table.
		/// </summary>
		/// <param name="tableName">Name of the join table</param>
		/// <param name="column">Column name on entity that will participate in many-to-many relationship</param>
		/// <param name="joinTableColumn">Join table column name on join table entity that will participate in many-to-many relationship</param>
		protected void HasJoinTable<TJOINTABLE>(string tableName,
		                                        Expression<Func<T, object>> column,
		                                        Expression<Func<TJOINTABLE, object>> joinTableColumn)
		{
			throw new NotImplementedException("Join table functionality not implemented yet...");
		}

		/// <summary>
		/// This will construct the representative data table for the entity to be set 
		/// in the <seealso cref="IMetadataStore">metadata storage</seealso> for resolving
		/// entities and columns to data tables and rows.
		/// </summary>
		/// <param name="metadataStore"></param>
		/// <returns></returns>
		public TableInfo Build(IMetadataStore metadataStore)
		{
			var tableInfo = new TableInfo(typeof (T), this.TableName, metadataStore, this.PrimaryKey, this.Columns,
			                              this.References, this.Collections, this.Components);
			return tableInfo;
		}

		private static PropertyInfo FindPropertyFromPropertyName(Type entity, string propertyName)
		{
			var property = (from match in entity.GetProperties()
			                where match.Name.Equals(propertyName)
			                select match).FirstOrDefault();
			return property;
		}

		private static string GetPropertyNameFromExpression<TEntity>(Expression<Func<TEntity, object>> expression)
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

		private static string GetPropertyNameForCollectionFromExpression<TEntity>(
			Expression<Func<TEntity, IEnumerable>> expression)
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
	}
}