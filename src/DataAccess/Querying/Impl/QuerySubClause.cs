using System;
using System.Linq.Expressions;
using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Impl
{
    internal class QuerySubClause<TEntity> : QueryClause
    {
        public QuerySubClausePrefix Prefix { get; private set; }

        public QuerySubClause(QuerySubClausePrefix prefix,
            IMetadataStore metadata, Expression<Func<TEntity, bool>> clause)
            : base( metadata, clause)
        {
            Prefix = prefix;
        }

        public override void Parse()
        {
            base.Parse();
            this.Criteria = string.Format("{0} {1}", this.Prefix.ToString().ToLower(), this.Criteria);
        }
    }
}