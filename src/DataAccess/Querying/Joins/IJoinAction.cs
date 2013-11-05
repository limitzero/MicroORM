using System.Text;

namespace MicroORM.DataAccess.Querying.Joins
{
	public interface IJoinAction
	{
		StringBuilder Build(StringBuilder builder);
	}
}