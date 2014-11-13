using MicroORM.Mapping;

namespace MicroORM.Tests.Domain.Models.Mapped
{
	public class Course
	{
		public Department Department { get; set; }

		private Course()
		{
		}

        /// <summary>
        /// Ideally this should be a non-public constructor as the department 
        /// is the only one designated to create a course...
        /// </summary>
        /// <param name="department"></param>
		internal Course(Department department)
		{
			Department = department;
		}

		public virtual int Id { get; set; }

		public virtual string Number { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }
	}

    public class CourseEntityMap : EntityMap<Course>
    {
        public CourseEntityMap()
        {
            TableName = "Course";
            HasPrimaryKey(p=>p.Id, "courseId");
            HasColumn(c => c.Number, "number");
            HasColumn(c => c.Name, "name");
            HasColumn(c => c.Description, "description");
            HasReference(c => c.Department);
        }
    }
}