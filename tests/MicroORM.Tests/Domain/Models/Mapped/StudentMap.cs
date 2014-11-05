using System;
using MicroORM.Mapping;

namespace MicroORM.Tests.Domain.Models.Mapped
{
	public class Student
	{
		public virtual int Id { get; set; }

		public virtual DateTime? EnrollmentDate { get; set; }

		public virtual Classification Classification { get; set; }

		public virtual Name Name { get; set; }

	    public Student()
	    {
            this.Name = new Name();
	    }
	}

	public class StudentMap : EntityMap<Student>
	{
		public StudentMap()
		{
			TableName = "Student";
			HasPrimaryKey(p => p.Id, "studentId");
			HasColumn(c => c.EnrollmentDate, "enrollmentdate");
			HasColumn(c => c.Classification, "classification");
			HasComponent(c => c.Name,
			             WithColumn(c => c.Name.FirstName, "firstname"),
			             WithColumn(c => c.Name.LastName, "lastname"));
		}
	}
}