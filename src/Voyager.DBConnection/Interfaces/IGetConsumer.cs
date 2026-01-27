using System;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Obsolete: Use IResultsConsumer instead.
	/// This interface is maintained for backward compatibility and will be removed in version 5.0.
	/// </summary>
	/// <typeparam name="TDomainObject">The type of domain object produced by this consumer.</typeparam>
	[Obsolete("Use IResultsConsumer<TDomainObject> instead. This interface will be removed in version 5.0.", false)]
	public interface IGetConsumer<TDomainObject> : IResultsConsumer<TDomainObject>
	{
	}
}
