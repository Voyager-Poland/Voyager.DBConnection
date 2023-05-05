﻿namespace Voyager.DBConnection.Exceptions
{
	[global::System.Serializable]
	public class SqlConnectionException : DBConnectionException
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public SqlConnectionException() { }
		public SqlConnectionException(string message) : base(message) { }
		public SqlConnectionException(string message, Exception inner) : base(message, inner) { }
		protected SqlConnectionException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
