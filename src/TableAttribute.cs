using System;

namespace MicroORM
{
	/// <summary>
	/// Attribute describing which data table that the entity represents.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{
		public TableAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets the name of the data column indicating the primary key for the entity.
		/// </summary>
		public string Name { get; private set; }
	}
}