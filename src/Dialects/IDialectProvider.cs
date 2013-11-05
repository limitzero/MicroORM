using System;
using System.Collections.Generic;

namespace MicroORM.Dialects
{
	public interface IDialectProvider
	{
		string BuildParameterAssignment(DialectComparisonOperator comparision, string tablename, string field,
		                                string literal = "");

		string BuildSelectStatement(string tableName, ICollection<string> fields);
		string BuildInsertStatement(string tableName, ICollection<string> fields);
		string BuildUpdateStatement(string tableName, ICollection<string> fields, string primaryKeyField);
		string BuildDeleteStatement(string tableName, string primaryKeyField);
		string GetIndentitySelectStatement(string tableName);
	}
}