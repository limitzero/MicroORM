using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MicroORM.DataAccess.Extensions
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Translates a list of objects to a delimited string list for display
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		public static string AsDelimitedList<T>(this IEnumerable<T> list, string delimiter)
		{
			StringBuilder builder = new StringBuilder();
			foreach (var item in list)
			{
				builder.AppendFormat("{0}{1}", item.ToString(), delimiter);
			}

			return builder.ToString().TrimEnd(delimiter.ToCharArray());
		}

		public static string CurrentStatement(this IEnumerable list, string statement)
		{
			return statement;
		}
	}
}