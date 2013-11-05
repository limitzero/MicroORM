using MicroORM.DataAccess.Internals;

namespace MicroORM
{
	public interface ISessionFactory
	{
		ISession OpenSession();
		ISession OpenSession(string connection);
		ISession OpenSessionViaAlias(string alias);
	}
}