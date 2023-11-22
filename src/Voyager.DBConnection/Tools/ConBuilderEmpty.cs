namespace Voyager.DBConnection.Tools
{
	class ConBuilderEmpty
	{
		private readonly string sqlConnectionString;

		public ConBuilderEmpty(string sqlConnectionString)
		{
			this.sqlConnectionString = sqlConnectionString;
		}
		public virtual string Prepare() => sqlConnectionString;
	}
}
