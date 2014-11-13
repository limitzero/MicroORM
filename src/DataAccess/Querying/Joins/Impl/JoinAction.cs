using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;

namespace MicroORM.DataAccess.Querying.Joins.Impl
{
	public abstract class JoinAction<TParentEntity, TChildEntity> : IJoinAction
		where TParentEntity : class, new()
		where TChildEntity : class, new()
	{
		protected Expression<Func<TParentEntity, object>> Parent { get; private set; }
		protected Expression<Func<TChildEntity, object>> Child { get; private set; }

		protected IMetadataStore MetadataStore { get; set; }
		protected Type ParentEntity { get; set; }
		protected Type JoinEntity { get; set; }

		protected JoinAction(IMetadataStore metadataStore)
		{
			MetadataStore = metadataStore;
		}

		public void Enqueue(Expression<Func<TParentEntity, object>> parent, Expression<Func<TChildEntity, object>> child)
		{
			this.MetadataStore.AddEntity(typeof (TParentEntity));
			this.MetadataStore.AddEntity(typeof (TChildEntity));
			this.Parent = parent;
			this.Child = child;
		}

		public StringBuilder Build(StringBuilder builder)
		{
			// build join statement exclusive of join type:
			string join = " [{0}] on [{0}].[{1}] = [{2}].[{3}]";

			var parentTableInfo = this.MetadataStore.GetTableInfo(typeof (TParentEntity));
			var childTableInfo = this.MetadataStore.GetTableInfo(typeof (TChildEntity));

			string parentPropertyName = this.MetadataStore.GetPropertyNameFromExpression<TParentEntity>(this.Parent);
			string childPropertyName = this.MetadataStore.GetPropertyNameFromExpression<TChildEntity>(this.Child);

			var parentColumns = new List<ColumnInfo>(parentTableInfo.Columns);
			parentColumns.Add(parentTableInfo.PrimaryKey);

			// get the reference to the parent on the child for joining:
			var childColumns = new List<ColumnInfo>(childTableInfo.Columns);
			childColumns.Add(childTableInfo.PrimaryKey);

			string parentDataColumnName = (from columnInfo in parentColumns
			                               where columnInfo.Column.Name == parentPropertyName
			                               select columnInfo.DataColumnName).FirstOrDefault();

			string childDataColumnName = (from columnInfo in childTableInfo.References
			                              let referenceTableInfo =
			                              	this.MetadataStore.GetTableInfo(columnInfo.Column.PropertyType)
			                              where referenceTableInfo.PrimaryKey.Column.Name == childPropertyName
			                              select referenceTableInfo.PrimaryKey.DataColumnName).FirstOrDefault();

			string statement = string.Format(join, childTableInfo.TableName, childDataColumnName,
			                                 parentTableInfo.TableName, parentDataColumnName);

			// this will include the type of join (i.e qualify the join type):
			string qualification = this.QualifyStatement(statement);

			if (string.Compare(qualification, statement, StringComparison.InvariantCulture) == 1)
				builder.AppendLine(qualification);
			else
			{
				builder.Append(statement);
			}

			return builder;
		}

		public abstract string QualifyStatement(string statement);
	}
}