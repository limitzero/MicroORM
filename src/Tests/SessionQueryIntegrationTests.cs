using System;
using System.Collections.Generic;
using System.Transactions;
using MicroORM.DataAccess;
using MicroORM.Dialects.Impl.SQLServer;
using Xunit;

namespace MicroORM.Tests
{
	public class SessionQueryIntegrationTests : IDisposable
	{
		private ISessionFactory factory;

		public SessionQueryIntegrationTests()
		{
			MicroORM.Configuration.Instance.DialectProvider<SQLServerDialectProvider>(
				new SQLServerDialectConnectionProvider(@".\SQLEXPRESS", "Contoso"));

			this.factory = MicroORM.Configuration.Instance.BuildSessionFactory();
		}

		public void Dispose()
		{
			this.factory = null;
		}

		[Fact]
		public void can_get_entity_by_identifier()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.Get<Department>(1);
				Assert.NotNull(department);
				Assert.Equal(1, department.Id);
			}
		}

		[Fact]
		public void can_insert_entity_when_identifier_is_not_set()
		{
			using (var session = factory.OpenSession())
			{
				var department = new Department {Description = "Test", Name = "Test", Number = "Test"};
				session.SaveOrUpdate(department);
				Assert.NotEqual(0, department.Id);
			}
		}

		[Fact]
		public void can_update_existing_entity_by_identifier()
		{
			using (var session = factory.OpenSession())
			{
				// insert:
				var department = new Department {Description = "Update", Name = "Update", Number = "Update"};
				session.SaveOrUpdate(department);

				// update:
				department.Description = "Update #2";
				session.SaveOrUpdate(department);

				var fromDB = session.Get<Department>(department.Id);
				Assert.Equal(fromDB.Description, department.Description);
			}
		}

		[Fact]
		public void can_find_entity_by_query()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.CreateQueryFor<Department>()
					.Select(SelectionOptions.AllFrom<Department>())
					.CreateCriteria(Restrictions.Like<Department>(d => d.Description, "#2"))
					.SingleOrDefault();

				Assert.NotNull(department);
			}
		}

		[Fact]
		public void can_find_entity_set_by_query()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.CreateQueryFor<Department>()
					.Select(SelectionOptions.AllFrom<Department>())
					.CreateCriteria(Restrictions.EqualTo<Department>(d => d.Id, 1))
					.SingleOrDefault();

				System.Diagnostics.Debug.WriteLine(department.Instructors.Count);
				Assert.True(department.Instructors.Count > 0);
			}
		}

		[Fact]
		public void can_use_aggregate_function_and_project_into_entity()
		{
			using (var session = factory.OpenSession())
			{
				var countOfNameView = session.CreateQueryFor<Department>()
					.Select(SelectionOptions.CountOnFieldFrom<Department>(d => d.Name, "name_count"))
					//.JoinOn<Instructor>(i=>i.Department.Id, d=>d.Id)
					.CreateCriteria(Restrictions.Like<Department>(d => d.Name, "arts"))
					.SingleOrDefault<CountOfNameView>();

				Assert.NotNull(countOfNameView);
				Assert.Equal(1, countOfNameView.Count);
			}
		}

		[Fact]
		public void can_use_session_with_dispose_pattern_to_execute_actions_in_transaction()
		{
			using (var session = factory.OpenSession())
			using(var txn = session.BeginTransaction())
			{
				try
				{
					// insert:
					var department = new Department
					                 	{
					                 		Description = "Update",
					                 		Name = "Update",
					                 		Number = "Update"
					                 	};

					session.SaveOrUpdate(department);

					// update:
					department.Description = "Update #2";
					session.SaveOrUpdate(department);

					// commit the transaction (if commit is never called, then changes will not happen):
					txn.Commit();

					var fromDB = session.Get<Department>(department.Id);
					Assert.True(department.Description.Equals(fromDB.Description));

				}
				catch (Exception operationException)
				{
					txn.Rollback();
					throw operationException;
				}
			}
		}

		[Fact]
		public void can_populate_component_properties_on_entity_from_select()
		{
			using (var session = factory.OpenSession())
			{
				// select done here and first and last name should be populated:
				var fromDB = session.Get<Instructor>(1);
				Assert.NotEqual(fromDB.Name.FirstName, string.Empty);
				Assert.NotEqual(fromDB.Name.LastName, string.Empty);
			}
		}

		[Fact]
		public void can_populate_component_properties_on_entity_from_insert()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.Get<Department>(1);
				var instructor = department.CreateInstructor();

				// "Name" is the component on instructor
				instructor.Name = new Name
				                  	{
				                  		FirstName = "test_component",
				                  		LastName = "test_component"
				                  	};

				session.Save(department);

				var fromDB = session.Get<Instructor>(instructor.Id);

				Assert.Equal(instructor.Name.FirstName, fromDB.Name.FirstName);
				Assert.Equal(instructor.Name.LastName, fromDB.Name.LastName);
			}
		}

		[Fact]
		public void can_populate_parent_collection_lazily_on_select()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.Get<Department>(1);

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
		public void can_get_entity_from_persistance_store_and_load_instance_again_from_sesion_cache()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.Get<Department>(1);
				var fromCache = session.Load<Department>(1);
				Assert.True(ReferenceEquals(department, fromCache));
			}
		}

		[Fact]
		public void can_execute_stored_procedure_and_map_result_into_view()
		{
			using (var session = factory.OpenSession())
			{
				var view = session.ExecuteProcedure()
					.SingleOrDefault<CountOfNameView>("pr_GetCount");

				Assert.NotNull(view);
				Assert.Equal(1000, view.Count);
			}
		}

		[Fact]
		public void can_use_enumeration_on_entity_for_insert_and_resolve_to_right_data_type_on_select()
		{
			using (var session = factory.OpenSession())
			{
				var student = new Student
				              	{
				              		Classification = Classification.Senior,
				              		Name = new Name
				              		       	{
				              		       		FirstName = "joe",
				              		       		LastName = "smart"
				              		       	},
				              		EnrollmentDate = System.DateTime.Now
				              	};

				session.Save(student);

				var fromDB = session.Get<Student>(student.Id);

				Assert.Equal(student.Id, fromDB.Id);
				Assert.Equal(student.Classification, fromDB.Classification);
			}
		}

		[Fact]
		public void can_intercept_entity_on_insert_and_change_data()
		{
			Configuration.Instance.RegisterInterceptor<DepartmentInterceptor>();

			using (var session = factory.OpenSession())
			{
				var department = new Department
				                 	{
				                 		Description = "Interceptor",
				                 		Name = "Interceptor",
				                 		Number = "Interceptor"
				                 	};

				session.SaveOrUpdate(department);

				var fromDB = session.Get<Department>(department.Id);
				Assert.Equal("inserted", fromDB.Name);
			}
		}
	}
}