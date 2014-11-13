using System.Collections.Generic;
using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.DataAccess.Querying.Impl;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
    public class UniqueResultAction<TEntity> : DatabaseAction<TEntity>
        where TEntity : class
    {
        private readonly IHydrator _hydrator;

        public UniqueResultAction(IMetadataStore metadataStore,
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect, IEnvironmentSettings environment) :
            base(metadataStore, default(TEntity), connection, dialect, environment)
        {
            this._hydrator = hydrator;
        }

        public TEntity GetSingleOrDefaultResult(string statement, ICollection<QueryParameter> parameters)
        {
            TEntity entity = default(TEntity);

            using ( var command = this.CreateCommand() )
            {
                command.CommandText = statement;
                command.CreateParametersFromQuery(parameters);

                // command.DisplayQuery();
                this.DisplayCommand(command);

                if ( this._hydrator != null )
                {
                    entity = _hydrator.HydrateEntity<TEntity>(command);

                    // force lazy loading on hydrated entity (if possible):
                    if ( entity != null )
                    {
                        if ( typeof(ILazyLoadSpecification).IsAssignableFrom(entity.GetType()) )
                        {
                            ( (ILazyLoadSpecification)entity ).IsLazyLoadingEnabled = true;
                        }
                    }
                }
            }

            return entity;
        }

        public TEntity GetSingleOrDefaultResult(string statement, IDictionary<string, object> parameters)
        {
            TEntity entity = default(TEntity);

            using ( var command = this.CreateCommand() )
            {
                command.CommandText = statement;
                command.CreateParametersFromDictionary(parameters);
                DisplayCommand(command);

                if ( this._hydrator != null )
                {
                    entity = _hydrator.HydrateEntity<TEntity>(command);

                    // force lazy loading on hydrated entity (if possible):
                    if ( entity != null )
                    {
                        if ( typeof(ILazyLoadSpecification).IsAssignableFrom(entity.GetType()) )
                        {
                            ( (ILazyLoadSpecification)entity ).IsLazyLoadingEnabled = true;
                        }
                    }
                }
            }

            return entity;
        }
    }
}