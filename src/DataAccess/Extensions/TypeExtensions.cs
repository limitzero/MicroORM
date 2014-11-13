using System;

namespace MicroORM.DataAccess.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsProxy(this Type type)
		{
		    if (type == null)
		        return false;

			return type.Name.EndsWith("Proxy");
		}
	}
}