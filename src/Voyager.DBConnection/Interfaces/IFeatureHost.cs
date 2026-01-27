using System;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a host that can manage and coordinate features for extending functionality.
	/// </summary>
	public interface IFeatureHost : IDisposable
	{
		/// <summary>
		/// Adds a feature to extend the functionality of the host.
		/// </summary>
		/// <param name="feature">The feature to add.</param>
		void AddFeature(IFeature feature);
	}
}
