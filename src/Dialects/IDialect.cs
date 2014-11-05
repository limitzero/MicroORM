using System.Data;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.Dialects
{
    /// <summary>
    /// This is the core contract that each database engine should inherit to adapt
    /// its convensions to the internal workings of the underlying session used for
    /// persistence and retreival of information.
    /// </summary>
    public interface IDialect
    {
        IDbConnection CreateConnection(string connectionString);
        string GetIdentityStatement(PrimaryKeyInfo primaryKeyInfo);
    }
}