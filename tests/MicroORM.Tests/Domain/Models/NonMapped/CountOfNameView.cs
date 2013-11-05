namespace MicroORM.Tests.Domain.Models.NonMapped
{
	public class CountOfNameView
	{
		[Column("name_count")]
		public int Count { get; set; }
	}
}