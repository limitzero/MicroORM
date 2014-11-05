using System;
using System.Data;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
    public class GetByIdAction<TEntity> : DatabaseAction<TEntity>
        where TEntity : class
    {
        private readonly IHydrator _hydrator;

        public GetByIdAction(IMetadataStore metadataStore,
            TEntity entity, IHydrator hydrator,
            IDbConnection connection, IDialect dialect)
            : base(metadataStore, entity, connection, dialect)
        {
            _hydrator = hydrator;
        }

        public TEntity GetById(object id)
        {
            TEntity entity = default(TEntity);

            using ( var command = this.CreateCommand() )
            {
                var tableInfo = this.MetadataStore.GetTableInfo<TEntity>();

                if ( tableInfo.PrimaryKey == null || tableInfo.PrimaryKey.Column == null )
                    throw new InvalidOperationException(
                        string.Format("The following entity '{0}' does not have a primary key assigned to a " +
                                      "data field for retreiving entity instances. Please assign the attribute '{1}' to the data property " +
                                      "that represents the primary key.",
                                      typeof(TEntity).FullName,
                                      typeof(PrimaryKeyAttribute).Name));

                if ( id.GetType() != tableInfo.PrimaryKey.Column.PropertyType )
                    throw new InvalidOperationException("The data value for the retreving the entity " + typeof(TEntity).FullName
                                                        + " does not match the type definition of " +
                                                        tableInfo.PrimaryKey.Column.PropertyType.FullName + " for the primary key.");

                var query = tableInfo.GetSelectStatmentForAllFields();
                query = tableInfo.AddWhereClauseById(query, id);
                command.CreateAndAddInputParameterForPrimaryKeyValue(tableInfo, tableInfo.PrimaryKey, id);
                command.CommandText = query;
                command.DisplayQuery();

                if ( _hydrator != null )
                    entity = _hydrator.HydrateEntity<TEntity>(command);

                // force lazy loading on hydrated entity (if possible):
                if ( entity != null )
                {
                    if ( typeof(ILazyLoadSpecification).IsAssignableFrom(entity.GetType()) )
                    {
                        ( (ILazyLoadSpecification)entity ).IsLazyLoadingEnabled = true;
                    }
                }

                return entity;
            }
        }
    }
}