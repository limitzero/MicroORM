using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using LinqExtender;
using LinqExtender.Ast;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Extensions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.DataAccess.LazyLoading;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
	public class QueryContext<T> : ExpressionVisitor, IQueryContext<T> where T : class, new()
	{
		private readonly IMetadataStore _metadataStore;
		private readonly IHydrator _hydrator;
        private readonly IDbConnection _connection;
	    private readonly IDialect _dialect;
	    private readonly IEnvironmentSettings _environment;
	    private readonly StringBuilder _builder;
		private readonly StringWriterReader _buffer;
		private readonly TableInfo _tableInfo;
		private readonly IDictionary<string, object> _parameters;
		private ColumnInfo _currentDataColumn;
		private Expression _currentExpression;

		public QueryContext(IMetadataStore metadataStore,
            IHydrator hydrator,
		    IDbConnection connection, 
            IDialect dialect, 
            IEnvironmentSettings environment)
		{
			_metadataStore = metadataStore;
			_hydrator = hydrator;
			_connection = connection;
		    _dialect = dialect;
		    _environment = environment;
		    this._builder = new StringBuilder();
			this._buffer = new StringWriterReader(this._builder);
			this._tableInfo = this._metadataStore.GetTableInfo<T>();
			this._parameters = new Dictionary<string, object>();
		}

		public string CurrentStatement
		{
			get
			{
				string statement = this._buffer.Read();
				return statement;
			}
		}

		public IEnumerable<T> Execute(LinqExtender.Ast.Expression expression)
		{
			this._currentExpression = expression;
			this.Visit(expression);

			var listingAction = new ToListAction<T>(this._metadataStore, 
                this._hydrator, this._connection, _dialect, _environment);

			var entities = listingAction.GetListing(this.CurrentStatement, this._parameters);
			return entities;
		}

		public override LinqExtender.Ast.Expression VisitTypeExpression(LinqExtender.Ast.TypeExpression expression)
		{
			IList<string> formattedFields = new List<string>();
			foreach (var field in this._tableInfo.GetFieldsForSelect())
			{
				formattedFields.Add(string.Format("[{0}].[{1}]", this._tableInfo.TableName, field));
			}

			Write(string.Format("select {0} from [{1}]",
			                    formattedFields.AsDelimitedList(","),
			                    this._tableInfo.TableName));

			return expression;
		}

		public override LinqExtender.Ast.Expression VisitLambdaExpression(LinqExtender.Ast.LambdaExpression expression)
		{
			WriteNewLine();
			Write(" where ");
			WriteNewLine();

			this.Visit(expression.Body);

			return expression;
		}

		public override LinqExtender.Ast.Expression VisitBinaryExpression(LinqExtender.Ast.BinaryExpression expression)
		{
			this.Visit(expression.Left);
			this.Write(GetBinaryOperator(expression.Operator));
			this.Visit(expression.Right);

			return expression;
		}

		public override LinqExtender.Ast.Expression VisitLogicalExpression(LinqExtender.Ast.LogicalExpression expression)
		{
			WriteTokenIfReq(expression, Token.LeftParenthesis);

			this.Visit(expression.Left);

			WriteLogicalOperator(expression.Operator);

			this.Visit(expression.Right);

			WriteTokenIfReq(expression, Token.RightParentThesis);

			return expression;
		}

		public override LinqExtender.Ast.Expression VisitMemberExpression(LinqExtender.Ast.MemberExpression expression)
		{
			string propertyName = expression.Member.Name;
			this._currentDataColumn = this._tableInfo.FindColumnForProperty(propertyName);

			this.Write(string.Format("[{0}].[{1}]",
			                         this._tableInfo.TableName,
			                         this._currentDataColumn.DataColumnName));
			return expression;
		}

		public override LinqExtender.Ast.Expression VisitLiteralExpression(LinqExtender.Ast.LiteralExpression expression)
		{
			object value = expression.Value;
			string parameter = this._currentDataColumn.DataColumnName;
			this._parameters.Add(parameter, value);
			WriteParameterForValue(string.Concat("@", parameter));
			return expression;
		}

		public override LinqExtender.Ast.Expression VisitOrderbyExpression(LinqExtender.Ast.OrderbyExpression expression)
		{
			WriteNewLine();
			Write(string.Format("order by {0}.{1} {2}",
			                    expression.Member.DeclaringType.Name,
			                    expression.Member.Name,
			                    expression.Ascending ? "asc" : "desc"));
			WriteNewLine();

			return expression;
		}

		private static string GetBinaryOperator(BinaryOperator @operator)
		{
			switch (@operator)
			{
				case BinaryOperator.Equal:
					return " = ";
				case BinaryOperator.GreaterThan:
					return " > ";
				case BinaryOperator.GreaterThanEqual:
					return " >= ";
				case BinaryOperator.LessThan:
					return " < ";
				case BinaryOperator.LessThanEqual:
					return " <= ";
				case BinaryOperator.NotEqual:
					return " <> ";
			}
			throw new ArgumentException("Invalid binary operator");
		}

		private void WriteLogicalOperator(LogicalOperator logicalOperator)
		{
			WriteSpace();

			this.Write(logicalOperator.ToString().ToUpper());

			WriteSpace();
		}

		private void WriteSpace()
		{
			this.Write(" ");
		}

		private void WriteNewLine()
		{
			this.Write(System.Environment.NewLine);
		}

		private void WriteTokenIfReq(LinqExtender.Ast.LogicalExpression expression, Token token)
		{
			if (expression.IsChild)
			{
				WriteToken(token);
			}
		}

		private void WriteToken(Token token)
		{
			switch (token)
			{
				case Token.LeftParenthesis:
					this.Write("(");
					break;
				case Token.RightParentThesis:
					this.Write(")");
					break;
			}
		}

		public enum Token
		{
			LeftParenthesis,
			RightParentThesis
		}

		private void WriteValue(TypeReference type, object value)
		{
			if (type.UnderlyingType == typeof (string))
				this.Write(String.Format("\"{0}\"", value));
			else
				this._buffer.Write(value);
		}

		private void WriteParameterForValue(string parameterName)
		{
			this._buffer.Write(parameterName);
		}

		private void Write(string value)
		{
			this._buffer.Write(value);
		}

		private IEnumerable<T> EnableLazyLoading(IEnumerable<T> entities)
		{
			foreach (var entity in entities)
			{
				// force lazy loading on hydrated entity (if possible):
				if (entity != null)
				{
					if (typeof (ILazyLoadSpecification).IsAssignableFrom(entity.GetType()))
					{
						((ILazyLoadSpecification) entity).IsLazyLoadingEnabled = true;
					}
				}
			}

			return entities;
		}
	}
}