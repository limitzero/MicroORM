namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Instructors")]
	public class Instructor
	{
		[PrimaryKey("instructorID")]
		public virtual int Id { get; set; }

		public virtual Department Department { get; set; }

		public virtual Name Name { get; set; }

		// needed by ORM (at least one parameter-less constructor)
		private Instructor()
		{
		}

		// an instructor must belong to a department:
		public Instructor(Department department)
		{
			Department = department;
		}
	}
}