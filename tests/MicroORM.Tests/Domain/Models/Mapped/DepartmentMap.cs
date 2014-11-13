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

        private readonly IList<Instructor> _instructors = new List<Instructor>();

        public virtual IList<Instructor> Instructors
        {
            get { return _instructors; }
        }

        private readonly IList<Course> _courses = new List<Course>();

        public virtual IList<Course> Courses
        {
            get { return _courses; }
        }

        public virtual Instructor CreateInstructor()
        {
            var instructor = new Instructor();
            _instructors.Add(instructor);
            return instructor;
        }

        // SRP : only a department can create a course
        public virtual Course CreateCourse(string courseNumber, string courseName, string courseDescription)
        {
            var course = new Course(this)
            {
                Number = courseNumber,
                Name = courseName,
                Description = courseDescription
            };

            if(_courses.Contains(course) == false)
                _courses.Add(course);

            return course;
        }
    }

    public class DepartmentMap : EntityMap<Department>
    {
        public DepartmentMap()
        {
            TableName = "Department";
            HasPrimaryKey(pk => pk.Id, "departmentId");
            HasColumn(c => c.Name, "name");
            HasColumn(c => c.Description, "description");
            HasCollection(c => c.Instructors);
            HasCollection(c => c.Courses);
        }
    }
}