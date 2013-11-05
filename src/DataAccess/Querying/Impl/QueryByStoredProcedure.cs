using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Impl
{
	public class QueryByStoredProcedure : IQueryByStoredProcedure
	{
		private IMetadataStore metadatastore;
		private IHydrator hydrator;
		private SqlConnection connection;

		public QueryByStoredProcedure(IMetadataStore metadatastore, IHydrator hydrator, SqlConnection connection)
		{
			this.metadatastore = metadatastore;
			this.hydrator = hydrator;
			this.connection = connection;
		}

		public TProjection SingleOrDefault<TProjection>(string procedure) where TProjection : class, new()
		{
			return this.SingleOrDefault<TProjection>(procedure, new Dictionary<string, object>());
		}

		public TProjection SingleOrDefault<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new()
		{
			this.metadatastore.AddEntity(typeof (TProjection));
			var action = new StoredProcedureToUniqueResultAction<TProjection>(this.metadatastore, this.hydrator, this.connection);
			return action.GetUniqueResult(procedure, parameters);
		}

		public IEnumerable<TProjection> ToList<TProjection>(string procedure) where TProjection : class, new()
		{
			return this.ToList<TProjection>(procedure, new Dictionary<string, object>());
		}

		public IEnumerable<TProjection> ToList<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new()
		{
			this.metadatastore.AddEntity(typeof (TProjection));
			var action = new StoredProcedureToListAction<TProjection>(this.metadatastore, this.hydrator, this.connection);
			return action.GetList(procedure, parameters);
		}
	}
}