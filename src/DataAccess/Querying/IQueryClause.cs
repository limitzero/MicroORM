using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MicroORM.DataAccess.Querying
{
    public interface IQueryClause<TParent, TEntity> 
        where TParent : class
        where TEntity : class
    {
        IQueryClause<TParent, TEntity> And<TOther>(Expression<Func<TOther, bool>> clause) where TOther : class;
        IQueryClause<TParent, TEntity> Or<TOther>(Expression<Func<TOther, bool>> clause) where TOther : class;
        IEnumerable<TParent> Select();
        IEnumerable<TProjection> Select<TProjection>() where TProjection : class;
    }
}