namespace MicroORM.Dialects
{
	public interface IDialectConnectionProvider
	{
		string GetConnectionString();
	}
}