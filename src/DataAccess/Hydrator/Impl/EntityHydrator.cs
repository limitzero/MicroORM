using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using LinqExtender.Abstraction;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.DataAccess.LazyLoading;

namespace MicroORM.DataAccess.Hydrator.Impl
{
	internal class EntityHydrator : IHydrator
	{
		private readonly Session session;
		private readonly ProxyGenerator generator;
		public IMetadataStore MetadataStore { get; private set; }

		public EntityHydrator(IMetadataStore metadataStore, Session session)
		{
			this.session = session;
			this.MetadataStore = metadataStore;
			this.generator = new ProxyGenerator();
		}

		public TEntity HydrateEntity<TEntity>(SqlCommand command) where TEntity : class
		{
			IDictionary<string, object> values = new Dictionary<string, object>();

			if (command.Connection != null)
			{
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows) return default(TEntity);

					reader.Read();
					values = GetValuesFromCurrentRow(reader);
				}
			}

			return CreateEntityFromValues<TEntity>(values);
		}

		public IEnumerable<TEntity> HydrateEntities<TEntity>(SqlCommand command) where TEntity : class
		{
			var rows = new List<IDictionary<string, object>>();
			var entities = new List<TEntity>();

			if (command.Connection != null)
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						rows.Add(GetValuesFromCurrentRow(reader));
					}
				}
			}

			foreach (var row in rows)
			{
				entities.Add(CreateEntityFromValues<TEntity>(row));
			}

			return entities;
		}

		public void UpdateEntity(Type targetPropertyType, Type targetToUpdate,
		                         object entityInstance, SqlCommand command)
		{
			Type targetChildType = null;
			var instanceType = entityInstance.GetType().IsProxy()
			                   	? entityInstance.GetType().BaseType
			                   	: entityInstance.GetType();

			if (targetPropertyType.IsGenericType == true)
				targetChildType = targetPropertyType.GetGenericArguments()[0];

			// Note: the target property type (if it is an enumerable should be changed to IEnumerable<> in calling code to reduce matching in LINQ statement
			var fieldToSet =
				(from match in instanceType
				 	.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				 where match.FieldType == targetPropertyType
				       || match.FieldType == typeof (ICollection<>).MakeGenericType(targetChildType)
				       || match.FieldType == typeof (IEnumerable<>).MakeGenericType(targetChildType)
				       || match.FieldType == typeof (List<>).MakeGenericType(targetChildType)
				 select match).FirstOrDefault();

			if (typeof (IEnumerable).IsAssignableFrom(targetPropertyType))
			{
				// collection property (child entities):
				var method = this.GetType().GetMethod("HydrateEntities");
				var genericMethod = method.MakeGenericMethod(new Type[] {targetToUpdate});
				var entities = genericMethod.Invoke(this, new object[] {command});

				// set the collection back on the entity instance (by field reference, not by property):
				if (fieldToSet != null)
				{
					fieldToSet.SetValue(entityInstance, entities);
				}
			}
			else
			{
				// reference property (associated entity):
				var method = this.GetType().GetMethod("HydrateEntity");
				var genericMethod = method.MakeGenericMethod(new Type[] {targetToUpdate});
				var entity = genericMethod.Invoke(this, new object[] {command});

				// set the associated entity on the existing entity:
				if (entity != null && fieldToSet != null)
				{
					fieldToSet.SetValue(entityInstance, entity);
				}
			}
		}

		public void InsertEntity<TEntity>(TEntity entity, SqlCommand command)
		{
			int id = 0;

			if (command.Connection != null)
			{
				var newRowId = command.ExecuteScalar();

				if (newRowId != DBNull.Value)
				{
					id = (int) newRowId;
				}
			}

			if (id > 0)
			{
				var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();
				entity.GetType().GetProperty(tableinfo.PrimaryKey.Column.Name).SetValue(entity, id, null);
			}
		}

		private IDictionary<string, object> GetValuesFromCurrentRow(SqlDataReader dataReader)
		{
			var values = new Dictionary<string, object>();

			for (int i = 0; i < dataReader.FieldCount; i++)
			{
				if (!values.ContainsKey(dataReader.GetName(i)))
					values.Add(dataReader.GetName(i), dataReader.GetValue(i));
			}
			return values;
		}

		private TEntity CreateEntityFromValues<TEntity>(IDictionary<string, object> values) where TEntity : class
		{
			// create the entity using the first instance of parameter-less constructor:
			var constructor = typeof (TEntity)
				.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.GetParameters().Length == 0)
				.Select(x => x).FirstOrDefault();

			var entity = constructor.Invoke(null) as TEntity;

			TEntity proxy = null;

			if (TryCreateLazyLoadedEntity(out proxy))
			{
				Hydrate(proxy as TEntity, values);
				return proxy as TEntity;
			}

			// hyadrate the non-proxied instance:
			Hydrate(entity, values);

			return entity;
		}

		private void Hydrate<TEntity>(TEntity entity, IDictionary<string, object> values)
		{
			var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();

			if (values.Count != 0)
			{
				var primarykey = tableinfo.PrimaryKey;

				if (primarykey != null)
				{
					if (string.IsNullOrEmpty(primarykey.DataColumnName) == false &&
					    values.ContainsKey(primarykey.DataColumnName))
					{
						SetColumnValue(primarykey, entity, values[primarykey.DataColumnName]);
					}
				}
			}

			SetRegularColumns(entity, tableinfo.Columns, values);
			SetComponentColumns(entity, tableinfo.Components, values);
		}

		private static void SetRegularColumns<TEntity>(TEntity entity,
		                                               IEnumerable<ColumnInfo> columns, IDictionary<string, object> values)
		{
			foreach (var column in columns)
			{
				if (column.Column.CanWrite)
				{
					if (values.Count == 0) continue;

					if (values.ContainsKey(column.DataColumnName))
					{
						object value = values[column.DataColumnName];

						if (column.IsNullable && value == DBNull.Value)
						{
							value = null;
						}
						else if (value == DBNull.Value)
						{
							value = null;
						}

						SetColumnValue(column, entity, value);
					}
				}
			}
		}

		private void SetComponentColumns<TEntity>(TEntity entity,
		                                          IEnumerable<ColumnInfo> columns, IDictionary<string, object> values)
		{
			foreach (var column in columns)
			{
				var component = Activator.CreateInstance(column.Column.PropertyType);
				var tableInfo = this.MetadataStore.GetTableInfo(column.Column.PropertyType);
				SetRegularColumns(component, tableInfo.Columns, values);
				SetColumnValue(column, entity, component);
			}
		}

		private static void SetColumnValue(ColumnInfo columninfo, object entity, object data)
		{
			if (columninfo.Column == null) return;

			if (columninfo.IsNullable && data == null)
			{
				columninfo.Column.SetValue(entity, null, null);
			}
			else if (data == null)
			{
				// skip loading...nothing we can do here
				return;
			}
			else
			{
				if (columninfo.Column.PropertyType.IsEnum)
				{
					object currentEnum = Enum.Parse(columninfo.Column.PropertyType, data.ToString());

					if (currentEnum != null)
					{
						columninfo.Column.SetValue(entity, currentEnum, null);
					}
				}
				else
				{
					columninfo.Column.SetValue(entity, data, null);
				}
			}
		}

		private bool TryCreateLazyLoadedEntity<TEntity>(out TEntity entity) where TEntity : class
		{
			bool success = false;
			entity = default(TEntity);

			try
			{
				var properties = this.GetLazyProperties<TEntity>();
				ILazyLoadSpecification lazyLoadMixin = new LazyLoadSpecification();
				lazyLoadMixin.Initialize(properties);

				// needed for LinqExtender library for LINQ implementation over entities:
				var queryObjectMixin = new QueryObjectImpl();

				ProxyGenerationOptions proxyGenerationOptions = new ProxyGenerationOptions();
				proxyGenerationOptions.AddMixinInstance(lazyLoadMixin);
				proxyGenerationOptions.AddMixinInstance(queryObjectMixin);

				var interceptor = new LazyLoadingInterceptor(this.session,
				                                             this.MetadataStore.GetTableInfo<TEntity>());

				entity = this.generator.CreateClassProxy<TEntity>(proxyGenerationOptions, interceptor);

				success = true;
			}
			catch
			{
				// TODO : log the exception that the entity could not have a lazy load proxy created for it			
			}

			return success;
		}

		private ICollection<string> GetLazyProperties<TEntity>()
		{
			var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

			var potentialLazyProperties = new List<string>();

			// collection entity associations:
			foreach (var collectionColumn in tableInfo.Collections)
			{
				if (collectionColumn.Column.CanRead)
				{
					potentialLazyProperties.Add(collectionColumn.Column.Name);
				}
			}

			// single entity associations:
			foreach (var reference in tableInfo.References)
			{
				if (reference.Column.CanRead)
				{
					potentialLazyProperties.Add(reference.Column.Name);
				}
			}

			return potentialLazyProperties;
		}
	}
}