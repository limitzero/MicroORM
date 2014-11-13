using System.Linq;
using LinqExtender;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Tests.Domain.Models.NonMapped;
using Xunit;

namespace MicroORM.Tests.Features.Querying
{
	public class LinqQueryQueryTests : BaseQueryTestFixture
	{
		private readonly ISession session;

		public LinqQueryQueryTests()
		{
			this.session = new Session();
		}

		[Fact]
		public void can_create_select_statement_from_linq_with_all_fields()
		{
            //var query = this.session.QueryOver<Account>()
            //    .Where(a => a.AccountNumber == "12345")
            //    .Select(account => account);

            //query.ToList().Take(500);

            //Assert.Equal(Expected(), Actual(query));
		}
	}
}