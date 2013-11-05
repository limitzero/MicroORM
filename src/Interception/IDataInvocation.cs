using MicroORM.DataAccess.Internals;

namespace MicroORM.Interception
{
	/// <summary>
	/// Instance that captures the data modification action for interception.
	/// </summary>
	public interface IDataInvocation
	{
		/// <summary>
		/// Gets the current session associated with the interception invocation.
		/// </summary>
		ISession Session { get; }

		/// <summary>
		/// Gets the current isntance that holds the meta-data for all entities.
		/// </summary>
		IMetadataStore MetadataStore { get; }

		/// <summary>
		/// Gets or sets the entity to be potentially modified during interception
		/// </summary>
		object Entity { get; set; }
	}
}