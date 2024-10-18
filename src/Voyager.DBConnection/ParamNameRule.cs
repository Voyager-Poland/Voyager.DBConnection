using System;

namespace Voyager.DBConnection
{
	internal class ParamNameRule
	{
		private readonly string paramToken;

		public ParamNameRule(string paramToken)
		{
			this.paramToken = paramToken;
		}
		public string GetParamName(string paramName)
		{
			if (paramName == null) throw new ArgumentNullException(nameof(paramName));


			if (!string.IsNullOrEmpty(paramToken))
				if (!paramName.StartsWith(paramToken))
				{
					return paramName.Insert(0, paramToken);
				}
			return paramName;
		}
	}

}