using System;
using System.Linq.Expressions;

namespace MicroORM.DataAccess.Querying
{
    public interface IQueryOver<TEntity> where TEntity : class, new()
    {
        IQueryOver<TEntity> Join<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new();

        IQueryOver<TEntity> LeftJoin<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new();

        IQueryOver<TEntity> RightJoin<TOther>(Expression<Func<TEntity, object>> entity,
            Expression<Func<TOther, object>> other)
            where TOther : class, new();

        IQueryClause<TEntity,TOther> Where<TOther>(Expression<Func<TOther, bool>> criteria)
            where TOther : class, new();
    }
}