using System;

namespace MicroORM
{
	/// <summary>
	/// Attribute describing which property will be the primary key on the entity.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PrimaryKeyAttribute : Attribute
	{
		public PrimaryKeyAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets the name of the data column indicating the primary key for the entity.
		/// </summary>
		public string Name { get; private set; }
	}
}