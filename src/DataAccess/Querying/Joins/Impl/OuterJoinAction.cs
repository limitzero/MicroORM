using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Joins.Impl
{
	public class OuterJoinAction<TParentEntity, TChildEntity> : JoinAction<TParentEntity, TChildEntity>
		where TParentEntity : class, new()
		where TChildEntity : class, new()
	{
		public OuterJoinAction(IMetadataStore metadataStore) :
			base(metadataStore)
		{
		}

		public override string QualifyStatement(string statement)
		{
			statement = string.Concat("outer join", statement);
			return statement;
		}
	}
}