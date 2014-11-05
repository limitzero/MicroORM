namespace MicroORM.Tests.Domain.Models
{
    // re-usable component (i.e. value type):
    public class Name
    {
        [Column("firstname")]
        public string FirstName { get; set; }

        [Column("lastname")]
        public string LastName { get; set; }

        public void Change(string firstName, string lastName)
        {
            if ( string.IsNullOrEmpty(FirstName) )
                this.FirstName = firstName;
            else if ( !this.FirstName.ToLower().Equals(firstName.ToLower()) )
                this.FirstName = firstName;

            if ( string.IsNullOrEmpty(LastName) )
                this.LastName = lastName;
            else if ( string.IsNullOrEmpty(this.LastName) == false &
                !this.LastName.ToLower().Equals(lastName.ToLower()) )
                this.LastName = lastName;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.LastName, this.FirstName);
        }
    }
}