using System.Collections.Generic;
using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
    public class StoredProcedureToUniqueResultAction<TEntity> : DatabaseAction<TEntity>
        where TEntity : class
    {
        private readonly IHydrator _hydrator;

        public StoredProcedureToUniqueResultAction(IMetadataStore metadataStore,
            IHydrator hydrator,
            IDbConnection connection,
             IDialect dialect, IEnvironmentSettings environment)
            : base(metadataStore, default(TEntity), connection, dialect, environment)
        {
            this._hydrator = hydrator;
        }

        public TEntity GetUniqueResult(string procedure, IDictionary<string, object> parameters)
        {
            TEntity entity = default(TEntity);

            using ( var command = this.CreateCommand() )
            {
                command.CommandText = procedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CreateParametersFromDictionary(parameters);

                // command.DisplayQuery();
                this.DisplayCommand(command);

                if ( this._hydrator != null )
                {
                    entity = _hydrator.HydrateEntity<TEntity>(command);
                }
            }

            return entity;
        }
    }
}