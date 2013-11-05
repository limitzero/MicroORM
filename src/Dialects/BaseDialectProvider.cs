using System;
using System.Collections.Generic;

namespace MicroORM.Dialects
{
	public abstract class BaseDialectProvider : IDialectProvider
	{
		protected const string FIELD_DELIMITER = ", ";
		protected const string FIELD_PARAMETER_DELIMITER = "@";

		public abstract string BuildParameterAssignment(DialectComparisonOperator comparision, string tablename, string field,
		                                                string literal = "");

		public abstract string BuildSelectStatement(string tableName, ICollection<string> fields);
		public abstract string BuildInsertStatement(string tableName, ICollection<string> fields);
		public abstract string BuildUpdateStatement(string tableName, ICollection<string> fields, string primaryKeyField);
		public abstract string BuildDeleteStatement(string tableName, string primaryKeyField);
		public abstract string GetIndentitySelectStatement(string tableName);

		public string BuildWhereClauseById(string tableName,
		                                   string dataColumnName,
		                                   string dataColumnAlias)
		{
			string queryWithClause = string.Format("WHERE [{0}].[{1}] = {2}",
			                                       tableName,
			                                       dataColumnName,
			                                       dataColumnAlias);

			return queryWithClause;
		}
	}
}