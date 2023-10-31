using System;

namespace Voyager.DBConnection.Interfaces
{
	public interface IFeatureHost : IDisposable
	{
		void AddFeature(IFeature feature);
	}
}
