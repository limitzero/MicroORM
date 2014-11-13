using System.Collections.Generic;
using System.Data;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
	public class QueryByStoredProcedure : IQueryByStoredProcedure
	{
		private readonly IMetadataStore _metadatastore;
		private readonly IHydrator hydrator;
		private readonly IDbConnection connection;
	    private readonly IDialect _dialect;
	    private readonly IEnvironmentSettings _environment;

	    public QueryByStoredProcedure(IMetadataStore metadatastore, 
            IHydrator hydrator, IDbConnection connection,
            IDialect dialect, IEnvironmentSettings environment)
		{
			this._metadatastore = metadatastore;
			this.hydrator = hydrator;
			this.connection = connection;
		    _dialect = dialect;
	        _environment = environment;
		}

		public TProjection SingleOrDefault<TProjection>(string procedure) where TProjection : class, new()
		{
			return this.SingleOrDefault<TProjection>(procedure, new Dictionary<string, object>());
		}

		public TProjection SingleOrDefault<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new()
		{
			this._metadatastore.AddEntity(typeof (TProjection));

            var action = new StoredProcedureToUniqueResultAction<TProjection>(
                this._metadatastore, this.hydrator, this.connection, _dialect, _environment);

			return action.GetUniqueResult(procedure, parameters);
		}

		public IEnumerable<TProjection> ToList<TProjection>(string procedure) where TProjection : class, new()
		{
			return this.ToList<TProjection>(procedure, new Dictionary<string, object>());
		}

		public IEnumerable<TProjection> ToList<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new()
		{
			this._metadatastore.AddEntity(typeof (TProjection));

            var action = new StoredProcedureToListAction<TProjection>(
                    this._metadatastore, this.hydrator, this.connection, _dialect, _environment);

			return action.GetList(procedure, parameters);
		}
	}
}