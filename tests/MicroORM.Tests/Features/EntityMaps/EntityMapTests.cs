using System;
using MicroORM.DataAccess;
using MicroORM.Tests.Domain.Models;
using MicroORM.Tests.Domain.Models.NonMapped;
using Xunit;
using Student = MicroORM.Tests.Domain.Models.Mapped.Student;
using Department = MicroORM.Tests.Domain.Models.Mapped.Department;

namespace MicroORM.Tests.Features.EntityMaps
{
	public class EntityMapTests : BaseSQLTestFixture
	{
		[Fact]
		public void can_use_entity_map_for_defining_entity_to_data_table_mapping()
		{
			using (var session = SessionFactory.OpenSession())
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
			using (var session = SessionFactory.OpenSession())
			{
				var department = session.Get<Department>(1);
				Assert.NotNull(department);

				// touching the instructors collection causes the lazy load:
				foreach (var instructor in department.Instructors)
				{
					System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2}",
					                                                 instructor.Id,
					                                                 instructor.Name.FirstName,
					                                                 instructor.Name.LastName));
				}

				var count = department.Instructors.Count;
				Assert.True(count > 0);
			}
		}

		[Fact]
		public void can_populate_component_properties_on_entity_from_insert()
		{
			using (var session = SessionFactory.OpenSession())
			using (var txn = session.BeginTransaction())
			{
				var department = session.Get<Department>(1);
				var instructor = department.CreateInstructor();
				instructor.ChangeDepartment(department);

				// "Name" is the component on instructor
				instructor.Name = new Name
				                  	{
				                  		FirstName = "test_component_from_entity_map",
				                  		LastName = "test_component_from_entity_map"
				                  	};

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
			using (var session = this.SessionFactory.OpenSession())
			{
				var department = session.CreateQueryFor<Department>()
					.Select(SelectionOptions.AllFrom<Department>())
					.CreateCriteria(Restrictions.Like<Department>(d => d.Description, "under"))
					.SingleOrDefault();

				Assert.NotNull(department);
			}
		}
	}
}