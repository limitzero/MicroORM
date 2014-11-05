using System;
using MicroORM.DataAccess.Internals;

namespace MicroORM
{
	public interface ISessionFactory : IDisposable
	{
		ISession OpenSession();
		ISession OpenSession(string connection);
		ISession OpenSessionViaAlias(string alias);
	}
}