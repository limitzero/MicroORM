using System;

namespace MicroORM.Tests.Domain.Models.NonMapped
{
	[Table("Students")]
	public class Student
	{
		[PrimaryKey("studentID")]
		public virtual int Id { get; set; }

		[Column("enrollmentdate")]
		public virtual DateTime? EnrollmentDate { get; set; }

		[Column("classificationID")]
		public virtual Classification Classification { get; set; }

		public virtual Name Name { get; set; }
	}
}