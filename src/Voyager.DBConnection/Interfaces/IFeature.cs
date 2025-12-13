using System;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a feature that can be added to extend the functionality of database executors.
	/// Features are disposable and can provide cross-cutting concerns such as logging, caching, or monitoring.
	/// </summary>
	public interface IFeature : IDisposable
	{
	}
}
