using System;

namespace MicroORM.DataAccess
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ReferencesAttribute : Attribute
	{
		public Type Entity { get; set; }

		public ReferencesAttribute(Type entity)
		{
			this.Entity = entity;
		}
	}
}