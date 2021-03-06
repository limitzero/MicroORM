using System;
using System.Linq;
using MicroORM.Configuration.Impl;
using MicroORM.Tests.Domain.Models;
using Xunit;
using Student = MicroORM.Tests.Domain.Models.Mapped.Student;
using Department = MicroORM.Tests.Domain.Models.Mapped.Department;
using Instructor = MicroORM.Tests.Domain.Models.Mapped.Instructor;

namespace MicroORM.Tests.Features.EntityMaps
{
	public class EntityMapTests : IDisposable
	{
	    private ISessionFactory _factory;
	    private const string Connection =@"Data Source=.\SQLEXPRESS;Initial Catalog=contoso;Integrated Security=SSPI";

	    public EntityMapTests()
	    {
	         var configuration = new EnvironmentConfiguration();
             _factory = configuration.BuildSessionFactory(Connection, this.GetType().Assembly);
	    }

	    public void Dispose()
	    {
            if(_factory != null)
                _factory.Dispose();
	        _factory = null;
	    }

	    [Fact]
		public void can_use_entity_map_for_defining_entity_to_data_table_mapping()
		{
			using (var session = _factory.OpenSession())
			using (var txn = session.BeginTransaction())
			{
				var student = new Student
				              	{
				              		Classification = Classification.Freshman,
				              		EnrollmentDate = System.DateTime.Now,
				              		Name = new Name
				              		       	{
				              		       		FirstName = "Joe",
				              		       		LastName = "Smith"
				              		       	}
				              	};

				try
				{
					session.SaveOrUpdate(student);
					txn.Commit();
				}
				catch
				{
					txn.Rollback();
					throw;
				}

				var fromDB = session.Get<Student>(student.Id);

				Assert.NotNull(fromDB);
				Assert.Equal(student.Id, fromDB.Id);
			}
		}

		[Fact]
		public void can_populate_parent_collection_lazily_on_select()
		{
            using ( var session = _factory.OpenSession() )
            {
                var department = new Department();
                department.Name = "Math";
                department.Number = "M100";
                department.Description = "Mathematics";

                var course = department.CreateCourse("101", "Introduction to Geometry",
                    "Teaches basic concepts and theory of geometry.");

                var anInstructor = department.CreateInstructor();
                anInstructor.Name.FirstName = "Joe";
                anInstructor.Name.LastName = "Smith"; 

                session.Save(department);

                var fromDb = session.Get<Department>(department.Id);

				// touching the instructors collection causes the lazy load:
                foreach ( var instructor in fromDb.Instructors )
				{
					System.Diagnostics.Debug.WriteLine("Instructor: {0} {1} {2}",
					                                                 instructor.Id,
					                                                 instructor.Name.FirstName,
					                                                 instructor.Name.LastName);
				}

				Assert.Equal(department.Instructors.Count,fromDb.Instructors.Count);
			}
		}

		[Fact]
		public void can_populate_component_properties_on_entity_from_insert()
		{
            using ( var session = _factory.OpenSession(Connection) )
			using (var txn = session.BeginTransaction())
			{
				var department = session.Get<Department>(1);
				var instructor = department.CreateInstructor();
				instructor.ChangeDepartment(department);

				// "Name" is the component on instructor
				instructor.ChangeName("test_component_from_entity_map",
				                  	     "test_component_from_entity_map");

				session.Save(department);

				txn.Commit(); // must commit the txn in order to use the Get<> below !!!

				var fromDB = session.Get<Instructor>(instructor.Id);

				Assert.Equal(instructor.Name.FirstName, fromDB.Name.FirstName);
				Assert.Equal(instructor.Name.LastName, fromDB.Name.LastName);
			}
		}

		[Fact]
		public void can_find_entity_by_query_when_entity_is_defined_by_mapping_class()
		{
            using ( var session = _factory.OpenSession(Connection) )
            using (var txn = session.BeginTransaction())
            {
                var department = new Department {Description = "Math", Name = "Math", Number = "101"};
                session.Save(department);
                txn.Commit();

                var fromQuery = session.QueryOver<Department>()
                    .Where((d) => d.Description == "Math")
                    .Select().FirstOrDefault();

                Assert.NotNull(fromQuery);
			}
		}
	}
}