using System.Collections.Generic;
using MicroORM.Mapping;

namespace MicroORM.Tests.Domain.Models.Mapped
{
	public class Department
	{
		public virtual int Id { get; set; }

		public virtual string Number { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }

		private IList<Instructor> instructors = new List<Instructor>();

		public virtual IList<Instructor> Instructors
		{
			get { return this.instructors; }
		}

		public virtual Instructor CreateInstructor()
		{
			var instructor = new Instructor();
			this.instructors.Add(instructor);
			return instructor;
		}
	}

	public class DepartmentMap : EntityMap<Department>
	{
		public DepartmentMap()
		{
			TableName = "Departments";
			HasPrimaryKey(pk => pk.Id, "departmentId");
			HasColumn(c => c.Name, "name");
			HasColumn(c => c.Description, "description");
			HasCollection(c => c.Instructors);
		}
	}
}