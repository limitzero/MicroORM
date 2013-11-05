using System;
using MicroORM.DataAccess.Internals;

namespace MicroORM.Interception.Impl
{
	internal class DataInvocation : IDataInvocation
	{
		private readonly Action proceedAction;
		public ISession Session { get; private set; }
		public IMetadataStore MetadataStore { get; private set; }
		public object Entity { get; set; }

		public DataInvocation(ISession session,
		                      IMetadataStore metadataStore,
		                      object entity,
		                      Action proceedAction)
		{
			this.proceedAction = proceedAction;
			Session = session;
			MetadataStore = metadataStore;
			Entity = entity;
		}

		public void Proceed()
		{
			if (this.proceedAction != null)
			{
				this.proceedAction();
			}
		}
	}
}