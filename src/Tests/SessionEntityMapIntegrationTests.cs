using System;
using System.Collections.Generic;
using MicroORM.DataAccess;
using MicroORM.Dialects.Impl.SQLServer;
using MicroORM.Mapping;
using Xunit;

namespace MicroORM.Tests
{
	public class SessionEntityMapIntegrationTests : IDisposable
	{
		private ISessionFactory factory;

		public SessionEntityMapIntegrationTests()
		{
			// SQL Server support for right now :()
			MicroORM.Configuration.Instance.DialectProvider<SQLServerDialectProvider>(
				new SQLServerDialectConnectionProvider(@".\SQLEXPRESS", "Contoso"));
			this.factory = MicroORM.Configuration.Instance.BuildSessionFactory(this.GetType().Assembly);
		}

		[Fact]
		public void can_use_entity_map_for_defining_entity_to_data_table_mapping()
		{
			using (var session = factory.OpenSession())
			using(var txn = session.BeginTransaction())
			{
				var student = session.Get<Student2>(1);
				Assert.NotNull(student);

				txn.Commit();
			}
		}

		[Fact]
		public void can_populate_parent_collection_lazily_on_select()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.Get<Department2>(1);
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
			using (var session = factory.OpenSession())
			using(var txn = session.BeginTransaction())
			{
				var department = session.Get<Department2>(1);
				var instructor = department.CreateInstructor();

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
		public void can_find_entity_by_query()
		{
			using (var session = factory.OpenSession())
			{
				var department = session.CreateQueryFor<Department2>()
					.Select(SelectionOptions.AllFrom<Department2>())
					.CreateCriteria(Restrictions.Like<Department2>(d => d.Description, "under"))
					.SingleOrDefault();

				Assert.NotNull(department);
			}
		}

		public void Dispose()
		{
			if (this.factory != null)
			{
				factory = null;
			}
		}

		public class Student2
		{
			public virtual int Id { get; set; }
			public virtual Name Name { get; set; }
			public virtual DateTime? EnrollmentDate { get; set; }
			public virtual Classification? Classification { get; set; }
		}

		public class Department2
		{
			public virtual int Id { get; set; }
			public virtual string Number { get; set; }
			public virtual string Name { get; set; }
			public virtual string Description { get; set; }

			private List<Instructor2> instructors = new List<Instructor2>();
			public virtual ICollection<Instructor2> Instructors
			{
				get { return this.instructors; }
			}

			public virtual Instructor2 CreateInstructor()
			{
				var instructor = new Instructor2(this);
				this.instructors.Add(instructor);
				return instructor;
			}
		}

		public class Instructor2
		{
			public virtual int Id { get; set; }
			public virtual Department2 Department { get; set; }
			public virtual Name Name { get; set; }

			// needed by ORM (at least one parameter-less constructor)
			private Instructor2()
			{
			}

			// an instructor must belong to a department:
			public Instructor2(Department2 department)
			{
				Department = department;
			}
		}

		//
		// database column names required in entity map here as to keep the entity "clean" from persistance:
		// 
		public class Department2EntityMap : EntityMap<Department2>
		{
			public Department2EntityMap()
			{
				this.TableName = "Departments";
				HasPrimaryKey(pk => pk.Id, "departmentID");
				HasColumn(c => c.Number, "number");
				HasColumn(c => c.Description, "description");
				HasColumn(c => c.Name, "name");
				HasCollection(c => c.Instructors);
			}
		}

		public class Instructor2EntityMap : EntityMap<Instructor2>
		{
			public Instructor2EntityMap()
			{
				this.TableName = "Instructors";
				HasPrimaryKey(pk => pk.Id, "instructorID");
				HasReference(r => r.Department);
				HasComponent(c => c.Name,
					WithColumn(c => c.Name.FirstName, "firstname"),
					WithColumn(c => c.Name.LastName, "lastname"));
			}
		}

		public class StudentEntityMap : EntityMap<Student2>
		{
			public StudentEntityMap()
			{
				TableName = "Students";
				HasPrimaryKey(pk => pk.Id, "studentID");
				HasComponent(c => c.Name,
							 WithColumn(c => c.Name.FirstName, "firstname"),
							 WithColumn(c => c.Name.LastName, "lastname"));
				HasColumn(c => c.EnrollmentDate, "enrollmentdate");
				HasColumn(c => c.Classification, "classificationID");
			}
		}

		/*
		public class Survey
		{
			[PrimaryKey("SurveyID")]
			public virtual int Id { get; set; }
		}

		public class Person
		{
			[PrimaryKey("PersonID")]
			public virtual int Id { get; set; }
		}

		public class SurveyMap :EntityMap<Survey>
		{
			public SurveyMap()
			{
				HasJoinTable<Tests.Person>("m_PersonSurveys", s =>s.Id,  p => p.Id);
			}
		}
		 
		 */ 
	}
}