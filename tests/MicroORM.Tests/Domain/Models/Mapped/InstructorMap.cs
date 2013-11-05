using System;
using MicroORM.Mapping;
using MicroORM.Tests.Domain.Models.NonMapped;

namespace MicroORM.Tests.Domain.Models.Mapped
{
	public class Instructor
	{
		public virtual int Id { get; set; }

		public virtual Department Department { get; set; }

		public virtual Name Name { get; set; }

		/// <summary>
		/// Sample business rule of how an instructor can be 
		/// changed to a different department.
		/// </summary>
		/// <param name="department"></param>
		public virtual void ChangeDepartment(Department department)
		{
			this.Department = department;
		}
	}

	public class InstructorMap : EntityMap<Instructor>
	{
		public InstructorMap()
		{
			TableName = "Instructors";
			HasPrimaryKey(pk => pk.Id, "instructorId");
			HasReference(r => r.Department);
			HasComponent(c => c.Name,
			             WithColumn(c => c.Name.FirstName, "firstname"),
			             WithColumn(c => c.Name.LastName, "lastname"));
		}
	}
}