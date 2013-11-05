using System.Collections.Generic;

namespace MicroORM.DataAccess.Querying
{
	public interface IQueryByStoredProcedure
	{
		TProjection SingleOrDefault<TProjection>(string procedure)
			where TProjection : class, new();

		TProjection SingleOrDefault<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new();

		IEnumerable<TProjection> ToList<TProjection>(string procedure)
			where TProjection : class, new();

		IEnumerable<TProjection> ToList<TProjection>(string procedure, IDictionary<string, object> parameters)
			where TProjection : class, new();
	}
}