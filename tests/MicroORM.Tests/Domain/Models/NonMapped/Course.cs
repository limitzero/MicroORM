using MicroORM.DataAccess;

namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Course")]
	public class Course
	{
		[References(typeof (Department))]
		public Department Department { get; set; }

		private Course()
		{
		}

		public Course(Department department)
		{
			Department = department;
		}

		[Column("courseID")]
		public virtual int Id { get; set; }

		[Column("number")]
		public virtual string Number { get; set; }

		[Column("name")]
		public virtual string Name { get; set; }

		[Column("description")]
		public virtual string Description { get; set; }
	}
}