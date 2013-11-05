namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Person")]
	public class Person
	{
		[Column("PersonId")]
		public virtual int Id { get; set; }

		[Column("nFirstName")]
		public virtual string FirstName { get; set; }

		[Column("nLastName")]
		public virtual string LastName { get; set; }
	}
}