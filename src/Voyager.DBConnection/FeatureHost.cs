using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	class FeatureHost : IFeatureHost
	{
		private readonly List<IFeature> featureList = new List<IFeature>();

		public void AddFeature(IFeature feature)
		{
			featureList.Add(feature);
		}

		public void Dispose()
		{
			foreach (IFeature feature in featureList)
				feature.Dispose();
		}
	}
}
