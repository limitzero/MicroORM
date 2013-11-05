using System.Collections.Generic;

namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Accounts")]
	public class Account
	{
		[PrimaryKey("AccountId")]
		public virtual int Id { get; set; }

		[Column("nAccountNumber")]
		public virtual string AccountNumber { get; set; }

		[Column("nDescription")]
		public virtual string Description { get; set; }

		// component!!
		public virtual Name Person { get; set; }

		private IList<AccountTransaction> transactions = new List<AccountTransaction>();

		public virtual IList<AccountTransaction> Transactions
		{
			get { return transactions; }
		}

		public virtual AccountTransaction CreateTransaction()
		{
			var transaction = new AccountTransaction(this);
			this.transactions.Add(transaction);
			return transaction;
		}
	}
}