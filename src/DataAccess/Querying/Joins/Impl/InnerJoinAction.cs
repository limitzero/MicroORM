using MicroORM.DataAccess.Internals;

namespace MicroORM.DataAccess.Querying.Joins.Impl
{
	public class InnerJoinAction<TParentEntity, TChildEntity> : JoinAction<TParentEntity, TChildEntity>
		where TParentEntity : class, new()
		where TChildEntity : class, new()
	{
		public InnerJoinAction(IMetadataStore metadataStore) :
			base(metadataStore)
		{
		}

		public override string QualifyStatement(string statement)
		{
			statement = string.Concat("join", statement);
			return statement;
		}
	}
}