using System;
using System.Collections.Generic;
using System.Linq;
using LinqExtender;
using MicroORM.DataAccess.Querying.Impl;
using ExpressionVisitor = MicroORM.DataAccess.Querying.Impl.ExpressionVisitor;

namespace MicroORM.Tests.Spikes.Querying
{
	public class TextContext<T> : ExpressionVisitor, IQueryContext<T>
	{
		public TextContext(ITextWriter writer)
		{
			this.writer = writer;
		}

		public IEnumerable<T> Execute(LinqExtender.Ast.Expression expression)
		{
			this.Visit(expression);
			return new List<T>().AsEnumerable();
		}

		public override LinqExtender.Ast.Expression VisitTypeExpression(LinqExtender.Ast.TypeExpression expression)
		{
			writer.Write(string.Format("select * from {0}", expression.Type.Name));
			return expression;
		}

		public override LinqExtender.Ast.Expression VisitLambdaExpression(LinqExtender.Ast.LambdaExpression expression)
		{
			WriteNewLine();
			writer.Write("where");
			WriteNewLine();

			this.Visit(expression.Body);

			return expression;
		}

		public override LinqExtender.Ast.Expression VisitBinaryExpression(LinqExtender.Ast.BinaryExpression expression)
		{
			this.Visit(expression.Left);
			writer.Write(GetBinaryOperator(expression.Operator));
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
			writer.Write(expression.FullName);
			return expression;
		}

		public override LinqExtender.Ast.Expression VisitLiteralExpression(LinqExtender.Ast.LiteralExpression expression)
		{
			WriteValue(expression.Type, expression.Value);
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
				case  BinaryOperator.LessThan:
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

			writer.Write(logicalOperator.ToString().ToUpper());

			WriteSpace();
		}

		private void WriteSpace()
		{
			writer.Write(" ");
		}

		private void WriteNewLine()
		{
			writer.Write(System.Environment.NewLine);
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
					writer.Write("(");
					break;
				case Token.RightParentThesis:
					writer.Write(")");
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
			if (type.UnderlyingType == typeof(string))
				writer.Write(String.Format("\"{0}\"", value));
			else
				writer.Write(value);
		}

		private void Write(string value)
		{
			writer.Write(value);
		}

		public ITextWriter writer;
		public bool parameter;
	}
}