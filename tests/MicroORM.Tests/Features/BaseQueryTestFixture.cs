using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MicroORM.DataAccess.Querying;
using MicroORM.DataAccess.Querying.Impl;

namespace MicroORM.Tests.Features
{
	public class BaseQueryTestFixture
	{
		protected string Expected()
		{
			// executing method:
			var frame = new StackTrace(1).GetFrame(0);

			var targetMethod = frame.GetMethod().Name;
			var expected = this.ReadResult(targetMethod);

			return expected;
		}

		protected string Actual(string content)
		{
			return Regex.Replace(content, "[\r\n\t]", string.Empty);
		}

		protected string Actual<T>(IQueryable<T> query) where T : class, new()
		{
			var context = query.GetType().GetField("context", BindingFlags.Instance | BindingFlags.NonPublic);
			var property = context.GetValue(query) as QueryContext<T>;
			var content = property.CurrentStatement;
			return Regex.Replace(content.Trim(), "[\r\n\t]", string.Empty);
		}

		private string ReadResult(string testname)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(String.Format("MicroORM.Tests.Cases.{0}.txt", testname)))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string content = reader.ReadToEnd();
					return Regex.Replace(content, "[\r\n\t]", string.Empty);
				}
			}
		}
	}
}