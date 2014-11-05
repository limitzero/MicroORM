using System.Collections.Generic;
using System.Data;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
    public class StoredProcedureToUniqueResultAction<TEntity> : DatabaseAction<TEntity>
        where TEntity : class
    {
        private readonly IHydrator hydrator;

        public StoredProcedureToUniqueResultAction(IMetadataStore metadataStore,
            IHydrator hydrator,
            IDbConnection connection,
             IDialect dialect)
            : base(metadataStore, default(TEntity), connection, dialect)
        {
            this.hydrator = hydrator;
        }

        public TEntity GetUniqueResult(string procedure, IDictionary<string, object> parameters)
        {
            TEntity entity = default(TEntity);

            using ( var command = this.CreateCommand() )
            {
                command.CommandText = procedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CreateParametersFromDictionary(parameters);
                command.DisplayQuery();

                if ( this.hydrator != null )
                {
                    entity = hydrator.HydrateEntity<TEntity>(command);
                }
            }

            return entity;
        }
    }
}