using System;

namespace MicroORM
{
	/// <summary>
	/// Attribute describing which data property represents a column in the persistance store for the entity.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		/// <summary>
		/// This will use the name of the property as the data column in the table.
		/// </summary>
		public ColumnAttribute()
		{
		}

		public ColumnAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets the name of the data column indicating the data column for the entity.
		/// </summary>
		public string Name { get; set; }
	}
}