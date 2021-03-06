﻿using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Joins.Impl
{
    public class RightJoinAction<TParentEntity, TChildEntity> : JoinAction<TParentEntity, TChildEntity>
        where TParentEntity : class, new()
        where TChildEntity : class, new()
    {
        public RightJoinAction(IMetadataStore metadataStore) :
            base(metadataStore)
        {
        }

        public override string QualifyStatement(string statement)
        {
            statement = string.Concat("right join", statement);
            return statement;
        }
    }
}