using System;
using System.Collections.Generic;
using MicroORM.DataAccess;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Interception;
using Xunit;

namespace MicroORM.Tests
{
	public class SessionQueryTests
	{
		private readonly ISession session;

		public SessionQueryTests()
		{
			session = new Session();
		}

		[Fact]
		public void can_create_session_query_for_select_all_over_object()
		{
			session.CreateQueryFor<Account>()
				.Select(SelectionOptions.FieldFrom<AccountTransaction>(t => t.Description),
						SelectionOptions.AvgOnFieldFrom<Account>(a => a.AccountNumber, "avg_accounts"))

				.LeftJoinOn<AccountTransaction>(t => t.Account.Id, a => a.Id)

				.CreateCriteria(Restrictions.CreateDisjunctionOn(
					Restrictions.EqualTo<Account>(a => a.Id, 1),
					Restrictions.GreaterThan<Account>(a => a.AccountNumber, "12344")),
					 Restrictions.Like<Account>(a => a.Description, "completed"))

				.AddOrder(OrderOptions.Asc<Account>(a => a.Id))
				.GroupBy(GroupByOptions.AliasField<Account>("avg_accounts"),
					 GroupByOptions.FieldFrom<AccountTransaction>(t => t.Description))

				.SingleOrDefault();
		}

		[Fact]
		public void can_generate_exception_on_creating_session_query_when_specified_query_value_does_not_match_property_type()
		{
			try
			{
				// account number is a string and an integer is used on input:
				session.CreateQueryFor<Account>()
					.Select(SelectionOptions.AllFrom<Account>())
					.CreateCriteria(Restrictions.EqualTo<Account>(a => a.AccountNumber, 123456))
					.SingleOrDefault();
				Assert.False(true, "The exception for mismatch query to property type was not thrown.");
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine(exception.ToString());
				Assert.Equal(typeof(InvalidCastException),  exception.GetBaseException().GetType());
			}

			//Assert.Throws<InvalidCastException>(() =>
			//  {
			//      // account number is a string and an integer is used on input:
			//      session.CreateQueryFor<Account>()
			//          .Select(SelectionOptions.AllFrom<Account>())
			//          .CreateCriteria(Restrictions.EqualTo<Account>(a => a.AccountNumber, 123456))
			//          .SingleOrDefault();
			//  }
			//);

		}

		[Fact]
		public void can_create_session_query_using_and_comparator()
		{
			session.CreateQueryFor<Account>()
				.Select(SelectionOptions.AllFrom<Account>())
				.CreateCriteria(Restrictions.EqualTo<Account>(a => a.AccountNumber, "12234"))
				.SingleOrDefault();
		}

		[Fact]
		public void can_create_session_query_using_and_comparator_with_component()
		{
			session.CreateQueryFor<Employee>()
				.Select(SelectionOptions.AllFrom<Employee>())
				.CreateCriteria(Restrictions.GreaterThanOrEqualTo<Employee>(a => a.EmployeeNumber, "12234"))
				.SingleOrDefault();
		}

		[Fact]
		public void can_create_update_statement_for_entity_when_primary_key_is_set()
		{
			var account = new Account
							{
								Id = 1,
								AccountNumber = "123456",
								Description = "This is a description"
							};
			session.Save(account);
		}

		[Fact]
		public void can_create_update_statement_for_entity_when_primary_key_is_set_and_component_is_defined()
		{
			var employee = new Employee
			{
				Id = 1,
				Name = new Name
						  {
							  FirstName = "joe",
							  LastName = "smith"
						  }
			};
			session.Save(employee);
		}

		[Fact]
		public void can_create_insert_statement_for_entity_when_primary_key_is_not_set_and_component_is_defined()
		{
			var employee = new Employee
			{
				Id = 0,
				Name = new Name
				{
					FirstName = "joe",
					LastName = "smith"
				}
			};
			session.Save(employee);
		}

		[Fact]
		public void can_create_insert_statement_for_entity_when_primary_key_is_not_set()
		{
			var account = new Account { AccountNumber = "123456", Description = "This is a description" };
			session.SaveOrUpdate(account);
		}

		[Fact]
		public void can_create_insert_statements_for_entity_when_child_entity_is_added_to_collection_and_not_initialized()
		{
			var account = new Account { AccountNumber = "123456", Description = "This is a description" };
			var transaction = account.CreateTransaction();
			transaction.Id = 0; // not initialized
			transaction.Description = "Here is a transaction for the account " + account.AccountNumber;
			session.SaveOrUpdate(account);
		}

		[Fact]
		public void can_retrieve_entity_by_primary_key_identifier()
		{
			session.Get<Account>(1);
		}

		[Fact]
		public void can_issue_delete_statement_for_entity_when_primary_key_is_populated()
		{
			var account = new Account
			{
				Id = 1,
				AccountNumber = "123456",
				Description = "This is a description"
			};
			session.Delete(account);
		}

		[Fact]
		public void can_issue_insert_statement_for_referenced_entity_on_parent_entity()
		{
			var account = new Account
			{
				Id = 1,
				AccountNumber = "123456",
				Description = "This is a description",
				Person = new Name
							{
								FirstName = "Joe",
								LastName = "Smith"
							}
			};

			session.Save(account);
		}

		[Fact]
		public void can_detect_component_class_on_entity_and_display_component_members_as_fields_on_parent_entity()
		{
			// an employee has a name (the name is the component class of employee)
			// a component is a class that does not have an identity but is used 
			// to describe or enhance an entity.
			var employee = new Employee
							{
								Name = new Name
										{
											FirstName = "joe",
											LastName = "smith"
										}
							};

			session.SaveOrUpdate(employee);
		}

		[Fact]
		public void test()
		{
			var department = this.session.Get<Department>(1);

			department.Id = 1;
			department.Number = Guid.NewGuid().ToString();

			var instructor = department.CreateInstructor();

			instructor.Name = new Name
			{
				FirstName = "test",
				LastName = "test"
			};

			this.session.Save(department);
		}

		[Fact]
		public void can_use_enumeration_on_entity_for_insert_and_resolve_to_right_data_type()
		{
			var student = new Student
							{
								Classification = Classification.Senior,
								Name = new Name
										{
											FirstName = "joe",
											LastName = "smart"
										}
							};

			this.session.Save(student);
		}

		[Fact]
		public void can_execute_stored_procedure_and_map_result_into_view()
		{
			var view = session.ExecuteProcedure()
				.SingleOrDefault<CountOfNameView>("pr_GetCount");
		}

		[Fact]
		public void can_intercept_entity_on_insert_and_change_data()
		{
			Configuration.Instance.RegisterInterceptor<SampleInterceptor>();

			var employee = new Employee
			{
				Name = new Name
				{
					FirstName = "joe",
					LastName = "smith"
				}
			};

			session.SaveOrUpdate(employee);

			Assert.Equal("inserted", employee.Name.LastName);
		}

		[Fact]
		public void can_intercept_entity_on_update_and_change_data()
		{
			Configuration.Instance.RegisterInterceptor<SampleInterceptor>();

			var employee = new Employee
			{
				Id = 21,
				Name = new Name
				{
					FirstName = "joe",
					LastName = "smith"
				}
			};

			session.SaveOrUpdate(employee);

			Assert.Equal("updated", employee.Name.LastName);
		}

		[Fact]
		public void can_intercept_entity_on_delete_and_change_data()
		{
			Configuration.Instance.RegisterInterceptor<SampleInterceptor>();

			var employee = new Employee
			{
				Id = 21,
				Name = new Name
				{
					FirstName = "joe",
					LastName = "smith"
				}
			};

			session.Delete(employee);

			Assert.Equal(true, employee.IsDeleted);
		}
	}


	public class DepartmentInterceptor : 
		IInsertInterceptor, 
		IUpdateInterceptor, 
		IDeleteInterceptor
	{
		public bool OnPreInsert(IDataInvocation invocation)
		{
			if (invocation.Entity.GetType() == typeof(Department))
			{
				// change the name to a different value and proceed:
				((Department)invocation.Entity).Name = "inserted";
			}

			// return true to invoke the insert action on the entity:
			return true;
		}

		public void OnPostInsert(IDataInvocation invocation)
		{

		}

		public bool OnPreUpdate(IDataInvocation invocation)
		{
			return true;
		}

		public void OnPostUpdate(IDataInvocation invocation)
		{

		}

		public bool OnPreDelete(IDataInvocation invocation)
		{
			return true;
		}

		public void OnPostDelete(IDataInvocation invocation)
		{

		}
	}

	public class SampleInterceptor : IInsertInterceptor, IUpdateInterceptor, IDeleteInterceptor
	{
		public bool OnPreInsert(IDataInvocation invocation)
		{
			if (invocation.Entity.GetType() == typeof(Employee))
			{
				// change the last name to a different value and proceed:
				((Employee)invocation.Entity).Name.LastName = "inserted";
			}

			// return true to invoke the insert action on the entity:
			return true;
		}

		public void OnPostInsert(IDataInvocation invocation)
		{

		}

		public bool OnPreUpdate(IDataInvocation invocation)
		{
			if (invocation.Entity.GetType() == typeof(Employee))
			{
				// change the last name to a different value and proceed:
				((Employee)invocation.Entity).Name.LastName = "updated";
			}

			return true;
		}

		public void OnPostUpdate(IDataInvocation invocation)
		{

		}

		public bool OnPreDelete(IDataInvocation invocation)
		{
			if (invocation.Entity.GetType() == typeof(Employee))
			{
				// mark the entity as deleted, but do not call proceed
				// or we will remove the entity from the persistance store,
				// we will update the entity in the persistance store with
				// a custom flag instead:
				((Employee)invocation.Entity).IsDeleted = true;
				invocation.Session.Save(invocation.Entity as Employee);
			}

			// do not proceed with the delete action:
			return false;
		}

		public void OnPostDelete(IDataInvocation invocation)
		{
			// never called when OnPreDelete returns false...
		}
	}

	public class CountOfNameView
	{
		[Column("name_count")]
		public int Count { get; set; }
	}

	[Table("Employee")]
	public class Employee
	{
		[PrimaryKey("employeeId")]
		public int Id { get; set; }

		[Column("employeeNumber")]
		public string EmployeeNumber { get; set; }

		public Name Name { get; set; }

		[Column("isDeleted")]
		public bool IsDeleted { get; set; }
	}

	// component (re-usable)
	public class Name
	{
		[Column("firstname")]
		public string FirstName { get; set; }

		[Column("lastname")]
		public string LastName { get; set; }

		public override string ToString()
		{
			return string.Format("{0}, {1}", this.LastName, this.FirstName);
		}
	}

	[Table("Departments")]
	public class Department
	{
		[PrimaryKey("departmentID")]
		public virtual int Id { get; set; }

		[Column("number")]
		public virtual string Number { get; set; }

		[Column("name")]
		public virtual string Name { get; set; }

		[Column("description")]
		public virtual string Description { get; set; }

		private IList<Instructor> instructors = new List<Instructor>();
		public virtual IList<Instructor> Instructors
		{
			get { return this.instructors; }
		}

		public virtual Instructor CreateInstructor()
		{
			var instructor = new Instructor(this);
			this.instructors.Add(instructor);
			return instructor;
		}
	}

	[Table("Instructors")]
	public class Instructor
	{
		[PrimaryKey("instructorID")]
		public virtual int Id { get; set; }

		public virtual Department Department { get; set; }

		public virtual Name Name { get; set; }

		// needed by ORM (at least one parameter-less constructor)
		private Instructor()
		{
		}

		// an instructor must belong to a department:
		public Instructor(Department department)
		{
			Department = department;
		}
	}


	[Table("Courses")]
	public class Course
	{
		[References(typeof(Department))]
		public Department Department { get; set; }

		private Course()
		{
		}

		public Course(Department department)
		{
			Department = department;
		}

		[Column("courseID")]
		public virtual int Id { get; set; }

		[Column("number")]
		public virtual string Number { get; set; }

		[Column("name")]
		public virtual string Name { get; set; }

		[Column("description")]
		public virtual string Description { get; set; }
	}

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

	public enum Classification
	{
		Freshman = 0,
		Sophmore = 1,
		Junior = 2,
		Senior = 3
	}


	// an account has zero or more transactions:

	[Table("Accounts")]
	public class Account
	{
		[PrimaryKey("AccountId")]
		public virtual int Id { get; set; }

		[Column("nAccountNumber")]
		public virtual string AccountNumber { get; set; }

		[Column("nDescription")]
		public virtual string Description { get; set; }

		// component!!
		public virtual Name Person { get; set; }

		private IList<AccountTransaction> transactions = new List<AccountTransaction>();
		public virtual IList<AccountTransaction> Transactions
		{
			get { return transactions; }
		}

		public virtual AccountTransaction CreateTransaction()
		{
			var transaction = new AccountTransaction(this);
			this.transactions.Add(transaction);
			return transaction;
		}
	}

	[Table("Transactions")]
	public class AccountTransaction
	{
		public Account Account { get; set; }

		[PrimaryKey("TransactionId")]
		public virtual int Id { get; set; }

		[Column("nDescription")]
		public virtual string Description { get; set; }

		public AccountTransaction()
		{
		}

		public AccountTransaction(Account account)
		{
			this.Account = account;
		}
	}

	[Table("Person")]
	public class Person
	{
		[Column("PersonId")]
		public virtual int Id { get; set; }

		[Column("nFirstName")]
		public virtual string FirstName { get; set; }

		[Column("nLastName")]
		public virtual string LastName { get; set; }

	}

}