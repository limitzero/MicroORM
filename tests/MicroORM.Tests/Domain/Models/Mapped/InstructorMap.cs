using MicroORM.Mapping;

namespace MicroORM.Tests.Domain.Models.Mapped
{
	public class Instructor
	{
		public virtual int Id { get; set; }

		public virtual Department Department { get; set; }

		public virtual Name Name { get; set; }

	    public Instructor()
	    {
            this.Name = new Name();
	    }

	    /// <summary>
		/// Sample business rule of how an instructor can be 
		/// changed to a different department.
		/// </summary>
		/// <param name="department"></param>
		public virtual void ChangeDepartment(Department department)
		{
			this.Department = department;
		}

	    public virtual void ChangeName(string firstName, string lastName)
	    {
            if(this.Name == null)
                this.Name = new Name();

            this.Name.Change(firstName, lastName);
	    }
	}

	public class InstructorMap : EntityMap<Instructor>
	{
		public InstructorMap()
		{
			TableName = "Instructor";
			HasPrimaryKey(pk => pk.Id, "instructorId");
			HasReference(r => r.Department);
			HasComponent(c => c.Name,
			             WithColumn(c => c.Name.FirstName, "firstname"),
			             WithColumn(c => c.Name.LastName, "lastname"));
		}
	}
}