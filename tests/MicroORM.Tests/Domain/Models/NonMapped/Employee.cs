namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Employee")]
	public class Employee
	{
		[PrimaryKey("employeeId")]
		public int Id { get; set; }

		[Column("employeeNumber")]
		public string EmployeeNumber { get; set; }

		public Name Name { get; set; }

		[Column("isDeleted")]
		public bool IsDeleted { get; set; }
	}
}