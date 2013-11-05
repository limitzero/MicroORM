using System.Collections.Generic;

namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Departments")]
	public class Department
	{
		[PrimaryKey("departmentID")]
		public virtual int Id { get; set; }

		[Column("number")]
		public virtual string Number { get; set; }

		[Column("name")]
		public virtual string Name { get; set; }

		[Column("description")]
		public virtual string Description { get; set; }

		private IList<Instructor> instructors = new List<Instructor>();

		public virtual IList<Instructor> Instructors
		{
			get { return this.instructors; }
		}

		public virtual Instructor CreateInstructor()
		{
			var instructor = new Instructor(this);
			this.instructors.Add(instructor);
			return instructor;
		}
	}
}