using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MicroORM.Configuration;
using MicroORM.DataAccess.Actions;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Querying.Impl
{
    internal class QueryClause<TParent, TEntity> : QueryClause, IQueryClause<TParent, TEntity> 
        where TParent : class
        where TEntity : class
    {
        private readonly QueryOverDefinition<TParent> _definition;
        private readonly Type _queriedOverEntity;
        private readonly IDbConnection _connection;
        private IHydrator _hydrator;
        private readonly IDialect _dialect;
        private readonly IEnvironmentSettings _environment;

        public QueryClause(QueryOverDefinition<TParent> definition,
            Type queriedOverEntity,
            IDbConnection connection,
            IMetadataStore metadataStore,
            IHydrator hydrator,
            IDialect dialect,
            IEnvironmentSettings environment,
            Expression<Func<TEntity, bool>> clause)
            : base(metadataStore, clause)
        {
            _definition = definition;
            _queriedOverEntity = queriedOverEntity;
            _connection = connection;
            _hydrator = hydrator;
            _dialect = dialect;
            _environment = environment;
        }

        public  IQueryClause<TParent, TEntity> And<TOther>(Expression<Func<TOther, bool>> clause)
            where TOther : class
        {
            var subClause = new QuerySubClause<TOther>(QuerySubClausePrefix.And, this.MetaDataStore, clause);
            this.SubClauses.Add(subClause);
            return this;
        }

        public IQueryClause<TParent, TEntity> Or<TOther>(Expression<Func<TOther, bool>> clause)
            where TOther : class
        {
            var subClause = new QuerySubClause<TOther>(QuerySubClausePrefix.Or, this.MetaDataStore, clause);
            this.SubClauses.Add(subClause);
            return this;
        }

        public IEnumerable<TParent> Select()
        {
            string statement = string.Empty;
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            _definition.Parse(out statement, out parameters);

            var listingAction = new ToListAction<TParent>(MetaDataStore,
                this._hydrator, this._connection, _dialect, _environment);

            var entities = listingAction.GetListing(statement, parameters);

            return entities;
        }

        public IEnumerable<TProjection> Select<TProjection>() where TProjection : class
        {
            string statement = string.Empty;
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            _definition.Parse(out statement, out parameters);

            return null;
        }
    }

    internal class QueryClause : System.Linq.Expressions.ExpressionVisitor
    {
        private readonly Expression _expression;
        private readonly StringBuilder _criteria = new StringBuilder();

        public IMetadataStore MetaDataStore { get; private set; }
        public List<QueryClause> SubClauses { get; private set; }

        public IDictionary<string, object> Parameters { get; private set; }
        public string Criteria { get; set; }

        protected QueryClause(IMetadataStore metadata, Expression expression)
        {
            MetaDataStore = metadata;
            _expression = expression;
            this.Parameters = new Dictionary<string, object>();
            this.SubClauses = new List<QueryClause>();
        }

        public virtual void Parse()
        {
            _criteria.Clear();
            Visit(_expression);
            this.Criteria = _criteria.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _criteria.Append("(");
            Visit(expression.Left);
            switch ( expression.NodeType )
            {
                case ExpressionType.And:
                    _criteria.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    _criteria.Append(" OR");
                    break;
                case ExpressionType.Equal:
                    if ( expression.Right.NodeType == ExpressionType.Constant )
                    {
                        var ce = (ConstantExpression)expression.Right;
                        if ( ce.Value == null )
                        {
                            _criteria.Append(" IS NULL)");
                            return expression;
                        }
                    }
                    _criteria.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    if ( expression.Right.NodeType == ExpressionType.Constant )
                    {
                        var ce = (ConstantExpression)expression.Right;
                        if ( ce.Value == null )
                        {
                            _criteria.Append(" IS NOT NULL)");
                            return expression;
                        }
                    }
                    _criteria.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    _criteria.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _criteria.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _criteria.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _criteria.Append(" >= ");
                    break;
                case ExpressionType.AndAlso:
                    _criteria.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    _criteria.Append(" OR ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported",
                        expression.NodeType));
            }
            Visit(expression.Right);
            _criteria.Append(")");
            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            string paramName = string.Format("@{0}", GetNextParameter());
            object paramValue = null;

            if ( c.Value == null )
            {
                _criteria.Append(paramName);
                paramValue = null;
            }
            else
            {
                switch ( Type.GetTypeCode(c.Value.GetType()) )
                {
                    case TypeCode.Boolean:
                        paramValue = (bool)c.Value ? 1 : 0;
                        _criteria.Append(paramName);
                        break;
                    case TypeCode.String:
                        paramValue = c.Value;
                        _criteria.Append(paramName);
                        break;
                    case TypeCode.DateTime:
                        _criteria.Append("convert(");
                        _criteria.Append("datetime");
                        _criteria.Append(", ");
                        _criteria.Append(paramName);
                        _criteria.Append(", 121)");
                        paramValue = c.Value;
                        break;
                    case TypeCode.Object:
                        // ReSharper disable OperatorIsCanBeUsed
                        if ( c.Value.GetType() == typeof(Guid) )
                            // ReSharper restore OperatorIsCanBeUsed
                        {
                            // force to ensure type
                            var g = new Guid(c.Value.ToString());
                            _criteria.Append(paramName);
                            paramValue = c.Value;
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported",
                                c.Value));
                        }
                        break;
                    default:
                        _criteria.Append(paramName);
                        paramValue = c.Value;
                        break;
                }
            }

            this.Parameters.Add(paramName, paramValue);

            return c;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ( m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where" )
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }

            if ( m.Method.Name == "Contains" && m.Arguments.Count == 1 )
            {
                var declaringType = m.Method.DeclaringType;
                if ( declaringType != null &&
                     declaringType.IsGenericType
                     && declaringType.GetGenericTypeDefinition() == typeof(ICollection<>) )
                {
                    _criteria.Append("(");
                    Visit(m.Arguments[0]);
                    _criteria.Append(" IN (");
                    _criteria.Append(ExtractCollection((ConstantExpression)m.Object));
                    _criteria.Append("))");
                }

                return m;
            }

            if ( m.Method.Name == "StartsWith" && m.Arguments.Count == 1 )
            {
                var constantExpression = m.Object as ConstantExpression;
                if ( constantExpression != null )
                {
                    _criteria.Append("(");
                    Visit(m.Arguments[0]);
                    _criteria.Append(" Like '");
                    _criteria.Append(constantExpression.Value.ToString());
                    _criteria.Append("%')");
                }
            }

            if ( m.Method.Name == "EndsWith" && m.Arguments.Count == 1 )
            {
                var constantExpression = m.Object as ConstantExpression;
                if ( constantExpression != null )
                {
                    _criteria.Append("(");
                    Visit(m.Arguments[0]);
                    _criteria.Append(" Like '%");
                    _criteria.Append(constantExpression.Value.ToString());
                    _criteria.Append("')");
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if ( m.Expression == null )
                return m;

            if ( m.Expression.NodeType == ExpressionType.Parameter )
            {
                string alias = AcquireAlias(m.Expression.Type, m.Expression.ToString());
                _criteria.Append(string.Format("{0}.", alias));
                _criteria.Append(m.Member.Name);
                return m;
            }
            else if ( m.Expression.NodeType == ExpressionType.MemberAccess )
            {
                ExtractExpression(m, m.Member.DeclaringType, m.Member.Name);
                //string alias = AcquireAlias(m.Expression.Type.DeclaringType, m.Expression.ToString());
                //_criteria.Append(string.Format("{0}.", alias));
                //_criteria.Append(m.Member.Name);
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));

        }

        private void ExtractExpression(MemberExpression expression, Type memberType, string memberName)
        {
            if ( expression == null )
                return;

            if ( expression.Expression != null & expression.Expression.NodeType == ExpressionType.Parameter )
            {
                var table = MetaDataStore.GetTableInfo(expression.Expression.Type);
                var component = MetaDataStore.GetTableInfo(memberType);
                var column = component.Columns.FirstOrDefault(c => c.Column.Name == memberName);

                var fullAlias = string.Format("[{0}].[{1}]",
                    table.TableName, column.DataColumnName);

                _criteria.Append(fullAlias);
                return;
            }
            else
            {
                ExtractExpression(expression.Expression as MemberExpression, memberType, memberName);
            }
        }

        private string AcquireAlias(Type type, string toString)
        {
            var table = MetaDataStore.GetTableInfo(type.DeclaringType);
            return string.Format("[{0}]", table.TableName);
        }

        private string GetNextParameter()
        {
            var index = Guid.NewGuid().ToString();
            var counter = index.Replace("-",string.Empty);
            var parameter = string.Format("_p{0}", counter);
            return parameter;
        }

        private static Expression StripQuotes(Expression e)
        {
            while ( e.NodeType == ExpressionType.Quote )
            {
                e = ( (UnaryExpression)e ).Operand;
            }
            return e;
        }

        private string ExtractCollection(ConstantExpression expression)
        {
            if ( expression.Value.GetType() == typeof(List<long>) )
            {
                return Compose((List<Int64>)expression.Value);
            }

            if ( expression.Value.GetType() == typeof(List<int>) )
            {
                return Compose((List<Int32>)expression.Value);
            }

            if ( expression.Value.GetType() == typeof(List<string>) )
            {
                return Compose((List<string>)expression.Value);
            }

            throw new NotSupportedException(string.Format("The constant for type '{0}' is not supported",
                expression.Value.GetType()));
        }

        private string Compose(IEnumerable<long> list)
        {
            return string.Join(",", list.ToArray());
        }

        private string Compose(IEnumerable<int> list)
        {
            return string.Join(",", list.ToArray());
        }

        private string Compose(IEnumerable<string> list)
        {
            return string.Join(",", list.ToArray());
        }
    }
}