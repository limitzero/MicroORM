using System;

namespace MicroORM.DataAccess.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsProxy(this Type type)
		{
			return type.Name.EndsWith("Proxy");
		}
	}
}