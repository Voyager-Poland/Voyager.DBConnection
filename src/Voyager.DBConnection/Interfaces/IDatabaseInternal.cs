using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Internal interface extending IDatabase with additional methods needed by command executors.
	/// This interface is used internally for dependency injection in helpers and is not part of the public API.
	/// </summary>
	internal interface IDatabaseInternal : IDatabase, IDisposable
	{
		/// <summary>
		/// Opens the connection and assigns it to the command.
		/// Also assigns the current transaction if one is active.
		/// </summary>
		/// <param name="cmd">The command to configure with connection and transaction.</param>
		void OpenCmd(DbCommand cmd);

		/// <summary>
		/// Begins a database transaction with the specified isolation level.
		/// </summary>
		/// <param name="isolationLevel">The isolation level for the transaction.</param>
		/// <returns>A Transaction object representing the active transaction.</returns>
		Transaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
	}
}
