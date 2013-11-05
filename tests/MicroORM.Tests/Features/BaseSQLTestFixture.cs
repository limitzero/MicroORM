using System;
using MicroORM.Dialects.Impl.SQLServer;

namespace MicroORM.Tests.Features
{
	/// <summary>
	/// Supporting SQL Server and SQL Express for testing and implementation.
	/// </summary>
	public class BaseSQLTestFixture : IDisposable
	{
		protected ISessionFactory SessionFactory { get; private set; }

		public BaseSQLTestFixture()
		{
			MicroORM.Configuration.Instance.DialectProvider<SQLServerDialectProvider>(
				new SQLServerDialectConnectionProvider(@".\SqlExpress", "contoso"));
			this.SessionFactory = MicroORM.Configuration.Instance.BuildSessionFactory(this.GetType().Assembly);
		}

		public virtual void Dispose()
		{
			if (this.SessionFactory != null)
			{
				this.SessionFactory = null;
			}
		}
	}
}