using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.Dialects.Impl.SQLServer
{
    public class SqlServerDialect : IDialect
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public string GetIdentityStatement(PrimaryKeyInfo primaryKeyInfo)
        {
           var statement = new StringBuilder();

            if (primaryKeyInfo.Column.PropertyType == typeof (long))
            {
              statement.Append("SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS BIGINT) AS ID");
            }
            else if (primaryKeyInfo.Column.PropertyType == typeof (int))
            {
                statement.Append("SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT) AS ID");
            }
            else if (primaryKeyInfo.Column.PropertyType == typeof (Guid))
            {
                statement.AppendFormat("SELECT CAST(ISNULL(SCOPE_IDENTITY(), '{0}') AS UNIQUEIDENTIFIER) AS ID", Guid.Empty);
            }

            return statement.ToString();
        }
    }
}