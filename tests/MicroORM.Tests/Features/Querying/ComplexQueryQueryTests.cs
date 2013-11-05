using MicroORM.DataAccess;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Tests.Domain.Models.NonMapped;
using Xunit;

namespace MicroORM.Tests.Features.Querying
{
	public class ComplexQueryQueryTests : BaseQueryTestFixture
	{
		private readonly ISession session;

		public ComplexQueryQueryTests()
		{
			this.session = new Session();
		}

		[Fact]
		public void can_create_select_statement_with_all_fields_and_bounded_result()
		{
			var query = this.session.CreateQueryFor<Account>();
			query.Select(SelectionOptions.AllFrom<Account>()).ToList();
			Assert.Equal(Expected(), Actual(query.CurrentStatement));
		}

		[Fact]
		public void can_create_select_statement_with_count_on_field()
		{
			var query = this.session.CreateQueryFor<Account>();
			query
				.Select(SelectionOptions.CountOnFieldFrom<Account>(f => f.AccountNumber, "count_of_account_number"))
				.SingleOrDefault();
			Assert.Equal(Expected(), Actual(query.CurrentStatement));
		}

		[Fact]
		public void can_create_select_statement_with_equal_restriction()
		{
			var query = this.session.CreateQueryFor<Account>();
			query
				.Select(SelectionOptions.AllFrom<Account>())
				.CreateCriteria(Restrictions.EqualTo<Account>(a => a.AccountNumber, "123456789"))
				.SingleOrDefault();
			Assert.Equal(Expected(), Actual(query.CurrentStatement));
		}

		[Fact]
		public void can_create_select_statement_with_like_restriction()
		{
			var query = this.session.CreateQueryFor<Account>();
			query
				.Select(SelectionOptions.AllFrom<Account>())
				.CreateCriteria(Restrictions.Like<Account>(a => a.AccountNumber, "123"))
				.SingleOrDefault();
			Assert.Equal(Expected(), Actual(query.CurrentStatement));
		}
	}
}