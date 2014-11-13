using System;
using System.Linq;
using MicroORM.Configuration.Impl;
using MicroORM.Tests.Domain.Models.Mapped;
using Xunit;

namespace MicroORM.Tests.Features.Querying
{
    public class QueryOverTests : IDisposable
    {
        private ISessionFactory _factory;
        private const string Connection = @"Data Source=.\SQLEXPRESS;Initial Catalog=contoso;Integrated Security=SSPI";

        public QueryOverTests()
        {
            var configuration = new EnvironmentConfiguration();
            _factory = configuration.BuildSessionFactory(this.GetType().Assembly);
        }

        public void Dispose()
        {
            if ( _factory != null )
            {
                _factory.Dispose();
            }
            _factory = null;
        }

        [Fact]
        public void it_should_be_able_to_construct_a_query()
        {
            using ( var session = _factory.OpenSession(Connection) )
            using ( var txn = session.BeginTransaction())
            {
                var department = new Department
                {
                    Description = "Test Query",
                    Name = "Test Query",
                    Number = "101"
                };

                var instructor = department.CreateInstructor();
                instructor.Name.Change("John", "Smith");

                session.Save(department);
                txn.Commit();

                var result = session.QueryOver<Department>()
                    .Join<Instructor>((d) => d.Id, (i) => i.Department.Id)
                    .Where<Instructor>((i) => i.Name.FirstName == "John")
                    .And<Instructor>((i) => i.Name.LastName == "Smith")
                    .Select().FirstOrDefault();

                Assert.NotNull(result);
            }

        }


    }
}