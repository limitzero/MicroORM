using System.Collections.Generic;
using System.Text;

namespace MicroORM.Dialects.Impl.SQLLite
{
	public class SQLLiteServerDialectProvider : BaseDialectProvider
	{
		public override string BuildParameterAssignment(DialectComparisonOperator comparision, string tablename, string field,
		                                                string literal = "")
		{
			string assignment = string.Empty;

			switch (comparision)
			{
				case DialectComparisonOperator.Equals:
					assignment = string.Format("[{0}].[{1}] = {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.EqualToNull:
					assignment = string.Format("[{0}].[{1}] is null", tablename, field);
					break;
				case DialectComparisonOperator.GreaterThan:
					assignment = string.Format("[{0}].[{1}] > {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.GreaterThanOrEqualTo:
					assignment = string.Format("[{0}].[{1}] >= {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.LessThan:
					assignment = string.Format("[{0}].[{1}] < {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.LessThanOrEqualTo:
					assignment = string.Format("[{0}].[{1}] <= {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.NotEquals:
					assignment = string.Format("[{0}].[{1}] <> {2}", tablename, field, string.Concat(FIELD_PARAMETER_DELIMITER, field));
					break;
				case DialectComparisonOperator.NotEqualToNull:
					assignment = string.Format("[{0}].[{1}] is not null", tablename, field);
					break;
				case DialectComparisonOperator.Like:
					//  using literal value on 'like' for SqlServer:
					assignment = string.Format("[{0}].[{1}] like '%{2}%'", tablename, field, literal);
					break;
			}

			return assignment;
		}

		public override string BuildSelectStatement(string tableName, ICollection<string> fields)
		{
			string fieldList = GetCommaSeparatedFieldNames(tableName, fields);
			string select = string.Format("SELECT {0} FROM [{1}]", fieldList, tableName);
			return select;
		}

		public override string BuildInsertStatement(string tableName, ICollection<string> fields)
		{
			// field names:
			string fieldNames = GetCommaSeparatedFieldNames(fields);
			string fieldParameterNames = GetCommaSeparatedParameterBasedFieldNames(fields);

			string insert = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2}); {3}",
			                              tableName,
			                              fieldNames,
			                              fieldParameterNames,
			                              this.GetIndentitySelectStatement(tableName));

			return insert;
		}

		public override string BuildUpdateStatement(string tableName,
		                                            ICollection<string> fields,
		                                            string primaryKeyField)
		{
			// updatable fields:
			StringBuilder builder = new StringBuilder();
			var updatableFields = this.GetFieldAndParameterBasedFieldCollection(fields);

			foreach (var updatableField in updatableFields)
			{
				if (updatableField.Key == primaryKeyField) continue;
				builder.Append(string.Format("{0} = {1}{2}",
				                             updatableField.Key,
				                             updatableField.Value,
				                             FIELD_DELIMITER));
			}

			var forUpdate = builder.ToString().TrimEnd(FIELD_DELIMITER.ToCharArray());

			string update = string.Format("UPDATE [{0}] SET {1} {2}",
			                              tableName,
			                              forUpdate,
			                              this.BuildWhereClauseById(tableName,
			                                                        primaryKeyField,
			                                                        string.Concat(FIELD_PARAMETER_DELIMITER, primaryKeyField)));

			return update;
		}

		public override string BuildDeleteStatement(string tableName,
		                                            string primaryKeyField)
		{
			string delete = string.Format("DELETE FROM [{0}] {1}", tableName,
			                              this.BuildWhereClauseById(tableName, primaryKeyField,
			                                                        string.Concat(FIELD_PARAMETER_DELIMITER, primaryKeyField)));
			return delete;
		}

		public override string GetIndentitySelectStatement(string tableName)
		{
			return "SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT) AS ID";
		}

		private IDictionary<string, string> GetFieldAndParameterBasedFieldCollection(ICollection<string> fields)
		{
			Dictionary<string, string> collection = new Dictionary<string, string>();

			new List<string>(fields).ForEach(field => collection.Add(field, string.Concat(FIELD_PARAMETER_DELIMITER, field)));

			return collection;
		}

		private string GetCommaSeparatedFieldNames(string tableName, ICollection<string> fields)
		{
			StringBuilder builder = new StringBuilder();
			new List<string>(fields).ForEach(field =>
			                                 builder.Append(string.Format("[{0}].[{1}]{2}",
			                                                              tableName, field, FIELD_DELIMITER)));
			string fieldList = builder.ToString().TrimEnd(FIELD_DELIMITER.ToCharArray());
			return fieldList;
		}

		private string GetCommaSeparatedFieldNames(ICollection<string> fields)
		{
			StringBuilder builder = new StringBuilder();
			new List<string>(fields).ForEach(field => builder.Append(string.Concat(field, FIELD_DELIMITER)));
			string fieldList = builder.ToString().TrimEnd(FIELD_DELIMITER.ToCharArray());
			return fieldList;
		}

		private string GetCommaSeparatedParameterBasedFieldNames(ICollection<string> fields)
		{
			StringBuilder builder = new StringBuilder();
			new List<string>(fields).ForEach(
				field => builder.Append(string.Concat(FIELD_PARAMETER_DELIMITER, field, FIELD_DELIMITER)));
			string fieldList = builder.ToString().TrimEnd(FIELD_DELIMITER.ToCharArray());
			return fieldList;
		}
	}
}