using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MicroORM.DataAccess.Internals.Impl
{
    public class TableInfo
    {
        private const string field_separator = ", ";

        private readonly bool isInitialized;

        /// <summary>
        /// Gets the entity that the table definition will be rendered for.
        /// </summary>
        public Type Entity { get; private set; }

        public IMetadataStore MetadataStore { get; set; }

        /// <summary>
        /// Gets the property implemented as the primary key on the entity.
        /// </summary>
        public PrimaryKeyInfo PrimaryKey { get; private set; }

        /// <summary>
        /// Gets the collection of simple data attributes for the entity.
        /// </summary>
        public ICollection<ColumnInfo> Columns { get; private set; }

        /// <summary>
        /// Gets the set of single entity references to the parent (implementation of "has a(n)" relationship)
        /// </summary>
        public ICollection<ColumnInfo> References { get; set; }

        /// <summary>
        /// Gets the set of child entity collections on the parent (implementation of "has zero or more" relationship)
        /// </summary>
        public ICollection<ColumnInfo> Collections { get; set; }

        /// <summary>
        /// Gets the collection of complex data attributes for the entity that are not entities themselves, but 
        /// are re-usable abstractions of a thing or concept.
        /// </summary>
        public IEnumerable<ColumnInfo> Components { get; private set; }

        /// <summary>
        /// Gets the name of the data table use to persist the entity values to.
        /// </summary>
        public string TableName { get; private set; }

        public TableInfo(Type table,
                         string tableName,
                         IMetadataStore metadataStore,
                         PrimaryKeyInfo primaryKey,
                         IEnumerable<ColumnInfo> columns,
                         IEnumerable<ColumnInfo> references,
                         IEnumerable<ColumnInfo> collections,
                         IEnumerable<ColumnInfo> components)
        {
            this.Entity = table;
            this.MetadataStore = metadataStore;
            this.TableName = tableName;
            this.PrimaryKey = primaryKey;
            this.Columns = new List<ColumnInfo>(columns);
            this.References = new List<ColumnInfo>(references);
            this.Collections = new List<ColumnInfo>(collections);
            this.Components = new List<ColumnInfo>(components);
            this.isInitialized = true;
            this.Initialize();
        }

        public TableInfo(Type table, IMetadataStore metadataStore)
            : this(table, null, metadataStore)
        {
        }

        public TableInfo(Type table, Type parent, IMetadataStore metadataStore)
        {
            MetadataStore = metadataStore;
            this.Entity = table;
            this.Columns = new List<ColumnInfo>();
            this.References = new List<ColumnInfo>();
            this.Collections = new List<ColumnInfo>();
            this.Components = new HashSet<ColumnInfo>();

            if ( parent != null )
            {
                this.MetadataStore.AddEntity(parent);
                this.TableName = this.MetadataStore.GetTableName(parent);
            }

            this.Initialize();
        }

        public ColumnInfo FindColumnForProperty(string propertyName)
        {
            ColumnInfo column = null;

            if ( this.PrimaryKey.Column.Name.Equals(propertyName) )
            {
                column = this.PrimaryKey;
                return column;
            }

            column = ( from match in this.Columns
                       where match.Column.Name.Equals(propertyName)
                       select match ).FirstOrDefault();

            if ( column != null )
                return column;

            column = ( from match in this.References
                       where match.Column.Name.Equals(propertyName)
                       select match ).FirstOrDefault();

            return column;
        }

        public ICollection<string> GetFieldsForSelect()
        {
            List<string> fields = new List<string>();

            // primary key field for mapping and hydration of entity:
            fields.Add(this.PrimaryKey.GetPrimaryKeyName());

            // regular columns:
            foreach ( var column in this.Columns )
            {
                if ( column.Column.CanRead )
                {
                    fields.Add(column.DataColumnName);
                }
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                this.MetadataStore.AddEntity(component.Column.PropertyType);
                var componentTableInfo = this.MetadataStore.GetTableInfo(this.Entity, component.Column.PropertyType);

                foreach ( var column in componentTableInfo.Columns )
                {
                    if ( column.Column.CanRead )
                    {
                        fields.Add(column.DataColumnName);
                    }
                }
            }

            // reference columns:
            foreach ( var column in this.References )
            {
                this.MetadataStore.AddEntity(column.Column.PropertyType);

                var referenceTableInfo = this.MetadataStore.GetTableInfo(column.Column.PropertyType);

                var primarykey = referenceTableInfo.PrimaryKey;

                if ( primarykey != null )
                {
                    if ( primarykey.Column.CanRead )
                    {
                        fields.Add(primarykey.GetPrimaryKeyName());
                    }
                }
            }

            return fields.Distinct().ToList();
        }

        public string GetSelectStatmentForAllFields()
        {
            const string statement = "SELECT {0} FROM [{1}] ";

            var fields = this.GetFieldsForSelect();

            var columns = string.Join(",",
                fields.Select(f => string.Format("[{0}].[{1}]",
                this.TableName, f)).ToList());

            var sql = string.Format(statement, columns, this.TableName);

            return sql;
        }

        public string GetInsertStatement(object entity)
        {
            const string statement = "INSERT INTO [{0}] ( {1} )  VALUES ( {2} )";

            var primaryKey = this.PrimaryKey.GetPrimaryKeyName();

            var fields = this.GetFieldsForSelect()
                .Where(field => !field.Equals(primaryKey))
                .Select(field => field)
                .ToList();

            var columns = string.Join(",", fields
                .Select(field => string.Format("[{0}]", field)).ToList());

            var values = string.Join(",",
                fields.Select(f => string.Format("@{0}", f)).ToList());

            var sql = string.Format(statement, this.TableName,
                columns,
                values);

            return sql;
        }

        public string GetDeleteStatement()
        {
            var statement = string.Format("DELETE FROM [{0}] ", this.TableName);
            return statement;
        }

        public string GetUpdateStatement(object entity)
        {
            const string statement = "UPDATE [{0}] SET {1} ";

            var primaryKey = this.PrimaryKey.GetPrimaryKeyName();

            var fields = this.GetFieldsForSelect()
                .Where(f => !f.Equals(primaryKey))
                .Select(f => f)
                .ToList();

            var assignments = string.Join(",",
                fields.Select(f => string.Format("[{0}].[{1}] = @{1}",
                    this.TableName, f)).ToList());

            var sql = string.Format(statement, this.TableName,
                assignments);

            return sql;
        }

        public string AddWhereClauseById(string query, object id)
        {
            var primaryKey = this.PrimaryKey.GetPrimaryKeyName();

            query = string.Format("{0} WHERE [{1}].[{2}] = @{2}",
                                  query,
                                  this.TableName,
                                  primaryKey);
            return query;
        }

        public string AddWhereClauseForParentById(PrimaryKeyInfo primaryKey, string query, object id)
        {
            query = string.Format("{0} WHERE [{1}].[{2}] = @{3}",
                                  query,
                                  this.TableName,
                                  primaryKey.DataColumnName,
                                  primaryKey.DataColumnName);
            return query;
        }

        public string GetPrimaryKeyValue<TEntity>(TEntity entity)
        {
            var data = this.PrimaryKey.Column.GetValue(entity, null);
            return CoalesceValue(data);
        }

        public bool IsPrimaryKeySet<TEntity>(TEntity entity)
        {
            var isKeySet = false;
            var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

            if ( tableInfo.PrimaryKey.Column.CanRead )
            {
                var data = tableInfo.PrimaryKey.Column.GetValue(entity, null);

                if ( data == null || data.GetType() == Guid.Empty.GetType() )
                {
                    return isKeySet;
                }

                if ( typeof(long).IsAssignableFrom(data.GetType()) )
                {
                    isKeySet = (long)data > 0;
                }

                if ( typeof(short).IsAssignableFrom(data.GetType()) )
                {
                    isKeySet = (short)data > 0;
                }

                if ( typeof(int).IsAssignableFrom(data.GetType()) )
                {
                    isKeySet = (int)data > 0;
                }

                if ( typeof(Guid).IsAssignableFrom(data.GetType()) )
                {
                    isKeySet = Guid.Empty.Equals((Guid)data);
                }

                if ( typeof(string).IsAssignableFrom(data.GetType()) )
                {
                    isKeySet = !string.IsNullOrEmpty((string)data);
                }
            }

            return isKeySet;
        }

        public string BuildValueAssignmentForUpdate(object entity)
        {
            StringBuilder builder = new StringBuilder();

            foreach ( var columnInfo in this.Columns )
            {
                if ( columnInfo == this.PrimaryKey || columnInfo.Column.PropertyType.IsGenericType )
                    continue;

                builder.Append(string.Format("[{0}] = {1}{2}",
                                             columnInfo.DataColumnName,
                                             CoalesePropertyValue(entity, columnInfo.Column),
                                             field_separator));
            }

            return TrimFieldsWithSeparator(builder.ToString());
        }

        public ICollection<IEnumerable> GetEntityCollections(object entity)
        {
            var collections = new List<IEnumerable>();
            var columnInfos = this.Columns.Where(x => x.Column.PropertyType.IsGenericType).ToList();

            foreach ( var columnInfo in columnInfos )
            {
                var genericEntity = columnInfo.Column.PropertyType.GetGenericArguments()[0];

                if ( genericEntity.IsClass )
                {
                    this.MetadataStore.AddEntity(genericEntity);

                    var collection = columnInfo.Column.GetValue(entity, null) as IEnumerable;

                    if ( collection != null )
                        collections.Add(collection);
                }
            }

            return collections;
        }

        public string CoalesceValue(object value)
        {
            string newValue = "is null";
            DateTime dateTime;
            Guid guid;

            if ( value.GetType().FullName.Contains("Nullable") )
            {
                dynamic nullable = value;
                object toCoalesce = null;

                if ( nullable.HasValue == true )
                {
                    toCoalesce = nullable.HasValue;
                    value = toCoalesce;
                }
            }

            var isDateParsed = DateTime.TryParse(value.ToString(), out dateTime);
            var isGuid = Guid.TryParse(value.ToString(), out guid);


            if ( typeof(string).IsAssignableFrom(value.GetType()) )
            {
                newValue = string.Format("{0}", value.ToString());
            }
            else if ( typeof(Guid).IsAssignableFrom(value.GetType()) ||
                     isGuid == true )
            {
                newValue = string.Format("'{0}'", value.ToString());
            }
            else if ( typeof(DateTime).IsAssignableFrom(value.GetType()) ||
                     isDateParsed == true )
            {
                if ( isDateParsed == false )
                {
                    dateTime = (DateTime)value;
                }

                newValue = string.Format("{0}", dateTime.ToString());
            }
            else if ( typeof(Enum).IsAssignableFrom(value.GetType()) )
            {
                newValue = ( (int)value ).ToString();
            }
            else if ( value == null )
            {
                newValue = "null";
            }
            else
            {
                newValue = value.ToString();
            }
            return newValue;
        }

        public static bool IsPropertyPopulated(ColumnInfo columnInfo, object entity)
        {
            bool isPopulated = false;

            if ( columnInfo.Column.CanRead )
            {
                var property = entity.GetType().GetProperty(columnInfo.Column.Name);

                if ( property != null )
                {
                    var propertyValue = property.GetValue(entity, null);

                    if ( propertyValue == null )
                        return isPopulated;

                    isPopulated = ( propertyValue != null || propertyValue.ToString() != string.Empty );
                }
            }

            return isPopulated;
        }

        public bool SupportsLazyLoading()
        {
            var supported = this.Collections.Any(c => c.IsLazyLoaded) ||
                            this.References.Any(r => r.IsLazyLoaded);
            return supported;
        }

        private void Initialize()
        {
            this.MetadataStore.AddEntity(this.Entity);

            if ( this.isInitialized )
                return;

            this.FindPrimaryKeyOnParent(this.Entity);

            if ( this.PrimaryKey == null || this.PrimaryKey.Column == null )
            {
                this.BuildEntityProjectionTableInfo();
            }
            else
            {
                this.BuildEntityTableInfo();
            }
        }

        /// <summary>
        /// This will map all data from a query into an entity that is defined 
        /// as a reference to a data table.
        /// </summary>
        private void BuildEntityTableInfo()
        {
            this.TableName = this.MetadataStore.GetTableName(this.Entity);
            this.Columns = this.FindAllDataPropertiesOnParent(this.Entity);
            this.References = this.FindAllSingleEntityReferencesOnParent(this.Entity);
            this.Collections = this.FindAllCollectionEntityReferencesOnParent(this.Entity);
            this.Components = this.FindAllComponentReferencesOnParent(this.Entity);
        }

        /// <summary>
        /// This will map all data from a query into an entity that is not 
        /// defined as a mapping to a table instance.
        /// </summary>
        private void BuildEntityProjectionTableInfo()
        {
            // projection (no table name, no primary key defined and no references defined):
            this.MetadataStore.AddEntity(this.Entity);
            this.Columns = this.FindAllDataPropertiesOnParent(this.Entity);
            this.Components = this.FindAllComponentReferencesOnParent(this.Entity);
        }

        private string GetColumnsWithParameterAssignment(object entity)
        {
            var builder = new StringBuilder();

            // data attributes:
            foreach ( var columnInfo in this.Columns )
            {
                if ( IsPropertyPopulated(columnInfo, entity) )
                {
                    builder.Append(string.Format("[{0}] = @{1}{2}",
                                                 columnInfo.DataColumnName,
                                                 columnInfo.DataColumnName,
                                                 field_separator));
                }
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                if ( IsPropertyPopulated(component, entity) )
                {
                    this.MetadataStore.AddEntity(component.Column.PropertyType);
                    var componentTableInfo = this.MetadataStore.GetTableInfo(component.Column.PropertyType);

                    var entityComponent = entity.GetType().GetProperty(component.Column.Name).GetValue(entity, null);

                    foreach ( var column in componentTableInfo.Columns )
                    {
                        if ( IsPropertyPopulated(column, entityComponent) )
                        {
                            builder.Append(string.Format("[{0}] = @{1}{2}",
                                                         column.DataColumnName,
                                                         column.DataColumnName,
                                                         field_separator));
                        }
                    }
                }
            }

            return TrimFieldsWithSeparator(builder.ToString());
        }

        private string BuildFieldsForSelect()
        {
            var fields = new StringBuilder();

            // primary key field for mapping and hydration of entity:
            fields.Append(string.Concat(string.Format("[{0}].[{1}]", this.TableName,
                                                      this.PrimaryKey.DataColumnName), field_separator));

            // regular columns:
            foreach ( var columnInfo in this.Columns )
            {
                if ( columnInfo.Column.CanRead )
                {
                    fields.Append(string.Concat(string.Format("[{0}].[{1}]", this.TableName,
                                                              columnInfo.DataColumnName), field_separator));
                }
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                this.MetadataStore.AddEntity(component.Column.PropertyType);
                var componentTableInfo = this.MetadataStore.GetTableInfo(this.Entity, component.Column.PropertyType);

                foreach ( var column in componentTableInfo.Columns )
                {
                    if ( column.Column.CanRead )
                    {
                        fields.Append(string.Concat(string.Format("[{0}].[{1}]", this.TableName,
                                                                  column.DataColumnName), field_separator));
                    }
                }
            }

            // reference columns:
            foreach ( var columnInfo in this.References )
            {
                this.MetadataStore.AddEntity(columnInfo.Column.PropertyType);

                var referenceTableInfo = this.MetadataStore.GetTableInfo(columnInfo.Column.PropertyType);

                var primarykey = referenceTableInfo.PrimaryKey;

                if ( primarykey != null )
                {
                    if ( primarykey.Column.CanRead )
                    {
                        fields.Append(string.Concat(string.Format("[{0}].[{1}]", this.TableName,
                                                                  primarykey.DataColumnName), field_separator));
                    }
                }
            }

            return TrimFieldsWithSeparator(fields.ToString());
        }

        private ICollection<string> GetFieldsForUpsert(object entity)
        {
            List<string> fields = new List<string>();

            // simple data columns on child:
            foreach ( var columnInfo in this.Columns )
            {
                if ( IsPropertyPopulated(columnInfo, entity) )
                {
                    fields.Add(columnInfo.DataColumnName);
                }
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                if ( IsPropertyPopulated(component, entity) )
                {
                    this.MetadataStore.AddEntity(component.Column.PropertyType);
                    var componentTableInfo = this.MetadataStore.GetTableInfo(component.Column.PropertyType);

                    var property = entity.GetType().GetProperty(component.Column.Name);
                    var entityComponent = property.GetValue(entity, null);

                    if ( entityComponent != null )
                    {
                        foreach ( var column in componentTableInfo.Columns )
                        {
                            if ( IsPropertyPopulated(column, entityComponent) )
                            {
                                fields.Add(column.DataColumnName);
                            }
                        }
                    }
                }
            }

            // parent entity column on child (entity reference):
            foreach ( var reference in this.References )
            {
                if ( IsPropertyPopulated(reference, entity) )
                {
                    var tableinfo = this.MetadataStore.GetTableInfo(reference.Column.PropertyType);
                    fields.Add(tableinfo.PrimaryKey.DataColumnName);
                }
            }

            return fields.Distinct().ToList();
        }

        private string BuildFieldsForInsert(object entity)
        {
            var fields = new StringBuilder();

            // simple data columns on child:
            foreach ( var columnInfo in this.Columns )
            {
                if ( IsPropertyPopulated(columnInfo, entity) )
                {
                    fields.Append(string.Concat(columnInfo.DataColumnName, field_separator));
                }
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                if ( IsPropertyPopulated(component, entity) )
                {
                    this.MetadataStore.AddEntity(component.Column.PropertyType);
                    var componentTableInfo = this.MetadataStore.GetTableInfo(component.Column.PropertyType);

                    var property = entity.GetType().GetProperty(component.Column.Name);
                    var entityComponent = property.GetValue(entity, null);

                    if ( entityComponent != null )
                    {
                        foreach ( var column in componentTableInfo.Columns )
                        {
                            if ( IsPropertyPopulated(column, entityComponent) )
                            {
                                fields.Append(string.Concat(column.DataColumnName, field_separator));
                            }
                        }
                    }
                }
            }

            // parent entity column on child (entity reference):
            foreach ( var reference in this.References )
            {
                if ( IsPropertyPopulated(reference, entity) )
                {
                    var tableinfo = this.MetadataStore.GetTableInfo(reference.Column.PropertyType);
                    fields.Append(string.Concat(tableinfo.PrimaryKey.DataColumnName, field_separator));
                }
            }

            string fieldlisting = string.Concat("(", TrimFieldsWithSeparator(fields.ToString()), ")");

            return fieldlisting;
        }

        private string TrimFieldsWithSeparator(string fields)
        {
            return fields.TrimEnd(string.Concat(field_separator).ToCharArray()).Trim();
        }

        private string BuildValuesClauseForInsert(object entity)
        {
            var fields = new StringBuilder();

            // simple data columns on child:
            foreach ( var columnInfo in this.Columns )
            {
                if ( IsPropertyPopulated(columnInfo, entity) )
                    fields.Append(string.Concat("@", columnInfo.DataColumnName, field_separator));
            }

            // components (expanded data attributes):
            foreach ( var component in this.Components )
            {
                if ( IsPropertyPopulated(component, entity) )
                {
                    this.MetadataStore.AddEntity(component.Column.PropertyType);
                    var componentTableInfo = this.MetadataStore.GetTableInfo(component.Column.PropertyType);

                    var property = entity.GetType().GetProperty(component.Column.Name);
                    var entityComponent = property.GetValue(entity, null);

                    foreach ( var column in componentTableInfo.Columns )
                    {
                        if ( IsPropertyPopulated(column, entityComponent) )
                        {
                            fields.Append(string.Concat("@", column.DataColumnName, field_separator));
                        }
                    }
                }
            }

            // parent entity column on child (entity reference):
            foreach ( var reference in this.References )
            {
                if ( IsPropertyPopulated(reference, entity) )
                {
                    var tableinfo = this.MetadataStore.GetTableInfo(reference.Column.PropertyType);
                    fields.Append(string.Concat("@", tableinfo.PrimaryKey.DataColumnName, field_separator));
                }
            }

            string fieldlisting = string.Concat(" VALUES (", TrimFieldsWithSeparator(fields.ToString()), ")");

            return fieldlisting;
        }

        private string CoalesePropertyValue(object entity, PropertyInfo property)
        {
            var data = property.GetValue(entity, null);
            return CoalesceValue(data);
        }

        private void FindPrimaryKeyOnParent(Type entity)
        {
            if ( entity.GetCustomAttributes(typeof(TableAttribute), false).Length > 0 )
            {
                var primaryKey = ( from property in this.MetadataStore.Entities[this.Entity]
                                   where property.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0
                                   select new PrimaryKeyInfo(this.MetadataStore, this.Entity, property) ).FirstOrDefault();
                this.PrimaryKey = primaryKey;
            }
        }

        /// <summary>
        /// This will extract all single entity references on the parent to an associated entity (i.e. an order has an order status)
        /// </summary>
        /// <param name="entity">Type to interrogate</param>
        /// <returns></returns>
        private ICollection<ColumnInfo> FindAllCollectionEntityReferencesOnParent(Type entity)
        {
            var collectionProperties = ( from property in this.MetadataStore.Entities[this.Entity]
                                         where property.PropertyType.IsGenericType
                                               && typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
                                         select property ).ToList().Distinct();

            // must be mapped to a table reference in order to be persisted:
            var columns = ( from property in collectionProperties
                            where
                             property.PropertyType.GetGenericArguments()[0].GetCustomAttributes(typeof(TableAttribute), false).
                                 Length > 0
                            select new ColumnInfo(this.MetadataStore, this.Entity, property) ).ToList().Distinct();

            return new List<ColumnInfo>(columns);
        }

        /// <summary>
        /// This will extract all single entity references on the parent to an associated entity (i.e. an order has an order status)
        /// </summary>
        /// <param name="entity">Type to interrogate</param>
        /// <returns></returns>
        private ICollection<ColumnInfo> FindAllSingleEntityReferencesOnParent(Type entity)
        {
            var columns = ( from property in this.MetadataStore.Entities[this.Entity]
                            where property.PropertyType.IsClass
                                  && property.PropertyType.IsAbstract == false
                                  && property.PropertyType.IsGenericType == false
                                  && property.PropertyType.GetCustomAttributes(typeof(TableAttribute), false).Length > 0
                                  && property.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length == 0
                            select new ColumnInfo(this.MetadataStore, this.Entity, property) ).ToList().Distinct();

            return new List<ColumnInfo>(columns);
        }

        /// <summary>
        /// This will extract out classes on the parent that constitute a grouping of 
        /// simliar attributes that can be re-used adding distinctiveness to the parent
        /// but not holding a lifecycle (i.e. a person has a name (first, last, middle) 
        /// and an employee has a name). The component is common and re-used but will 
        /// be persisted as part of the parent entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ICollection<ColumnInfo> FindAllComponentReferencesOnParent(Type entity)
        {
            var columns = ( from property in this.MetadataStore.Entities[this.Entity]
                            where property.PropertyType.IsClass
                                  && property.PropertyType.IsAbstract == false
                                  && property.PropertyType.IsGenericType == false
                                  && typeof(IEnumerable).IsAssignableFrom(property.PropertyType) == false
                                  && property.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length == 0
                                  && property.PropertyType.GetCustomAttributes(typeof(TableAttribute), false).Length == 0
                            select new ColumnInfo(this.MetadataStore, this.Entity, property) ).ToList().Distinct();

            return new HashSet<ColumnInfo>(columns);
        }

        /// <summary>
        /// This will extract base properties that are common to the .NET framework 
        /// that are useful in describing the entity. It will exclude all generic and 
        /// class types leaving only primitives (i.e. boolean, datetime, int, decimal, etc).
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ICollection<ColumnInfo> FindAllDataPropertiesOnParent(Type entity)
        {
            var columns = ( from property in this.MetadataStore.Entities[this.Entity]
                            where
                             ( property.PropertyType.FullName.StartsWith("System")
                              || property.PropertyType.IsEnum == true )
                             && property.PropertyType.IsGenericType == false
                             && property.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length == 0
                            select new ColumnInfo(this.MetadataStore, this.Entity, property) ).ToList().Distinct();

            var nullables = ( from property in this.MetadataStore.Entities[this.Entity]
                              where property.PropertyType.FullName.StartsWith("System.Nullable")
                                    && property.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length == 0
                              select new ColumnInfo(this.MetadataStore, this.Entity, property) ).ToList().Distinct();

            var dataColumns = new List<ColumnInfo>();

            dataColumns.AddRange(columns);
            dataColumns.AddRange(nullables);

            return dataColumns;
        }
    }
}