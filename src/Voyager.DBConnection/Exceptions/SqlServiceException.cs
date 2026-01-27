using System;

namespace Voyager.DBConnection.Exceptions
{
	[global::System.Serializable]
	public class SqlServiceException : DBConnectionException
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public SqlServiceException() { }
		public SqlServiceException(string message) : base(message) { }
		public SqlServiceException(string message, Exception inner) : base(message, inner) { }
	}
}
