using System;

namespace MicroORM
{
	/// <summary>
	/// Contract for scoping changes to the persistance store for a particular <seealso cref="ISession">session</seealso>
	/// </summary>
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// This will rollback the current set of actions against the persistance store for session data changes.
		/// </summary>
		void Rollback();

		/// <summary>
		/// This will commit the current set of actions against the persistance store for session data changes.
		/// </summary>
		void Commit();
	}
}