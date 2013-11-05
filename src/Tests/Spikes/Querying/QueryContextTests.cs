using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqExtender;
using MicroORM.DataAccess.Querying.Impl;
using MicroORM.Dialects.Impl.SQLServer;
using Xunit;

namespace MicroORM.Tests.Spikes.Querying
{
	public class QueryContextTests : IDisposable
	{
		private ISessionFactory _factory;

		public QueryContextTests()
		{
			MicroORM.Configuration.Instance.DialectProvider<SQLServerDialectProvider>(
				new SQLServerDialectConnectionProvider(@".\SQLEXPRESS", "Contoso"));

			// need to call this overload for using entities with entity map conventions:
			this._factory = MicroORM.Configuration.Instance.BuildSessionFactory(this.GetType().Assembly);
		}
		public void Dispose()
		{
			this._factory = null;
		}

		[Fact]
		public void can_create_query_statement_from_entity()
		{
			var builder = new StringBuilder();
			var context = new TextContext<Instructor>(new StringWriterReader(builder));

			var departments = new List<SessionEntityMapIntegrationTests.Department2>(); 

			var instructor = from match in context  
								    where match.Id == 1 
									select match;

			// force the expression tree to be evaluated:
			instructor.Count();

			System.Diagnostics.Debug.WriteLine(builder.ToString());
		}

		[Fact]
		public void can_use_session_to_query_over_entity()
		{
			using (var session = _factory.OpenSession())
			{
				var query =
					session.QueryOver<SessionEntityMapIntegrationTests.Department2>()
						.Where(d => d.Id == 1)
						.Select(d => d);

				// expression tree is evaluated here and SQL is generated against data store:
				var department = query.FirstOrDefault();

				Assert.NotNull(department);

				foreach (var instructor in department.Instructors)
				{
					System.Diagnostics.Debug.WriteLine(instructor.Name);
				}
			}
		}


		[Fact]
		public void can_use_session_to_query_over_entity_with_join_support()
		{
			var builder = new StringBuilder();
			var context = new TextContext<Blog>(new StringWriterReader(builder));
	
		}

		private bool Contains(string item, string value)
		{
			return item.Contains(value);
		}

		[Table("blogs")]
		public class Blog
		{
			[Column("blogid")]
			public int Id { get; set; }

			[Column("blog_name")]
			public string Name { get; set; }
		}

		[Table("posts")]
		public class Post
		{
			public Blog Blog { get; set; }

			[Column("postid")]
			public int Id { get; set; }

			[Column("post_name")]
			public string Name { get; set; }

			public Post()
			{
			}

			public Post(Blog blog)
			{
				this.Blog = blog;
			}
		}
	}
}