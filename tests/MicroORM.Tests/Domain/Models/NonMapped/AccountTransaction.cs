namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Transactions")]
	public class AccountTransaction
	{
		public Account Account { get; set; }

		[PrimaryKey("TransactionId")]
		public virtual int Id { get; set; }

		[Column("nDescription")]
		public virtual string Description { get; set; }

		public AccountTransaction()
		{
		}

		public AccountTransaction(Account account)
		{
			this.Account = account;
		}
	}
}