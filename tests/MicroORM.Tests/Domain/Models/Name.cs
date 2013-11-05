namespace MicroORM.Tests.Domain.Models
{
	// re-usable component:
	public class Name
	{
		[Column("firstname")]
		public string FirstName { get; set; }

		[Column("lastname")]
		public string LastName { get; set; }

		public override string ToString()
		{
			return string.Format("{0}, {1}", this.LastName, this.FirstName);
		}
	}
}