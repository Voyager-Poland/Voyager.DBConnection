﻿namespace Voyager.DBConnection.Exceptions
{
	[global::System.Serializable]
	public class SessionExpiredException : DBConnectionException
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public SessionExpiredException() { }
		public SessionExpiredException(string message) : base(message) { }
		public SessionExpiredException(string message, System.Exception inner) : base(message, inner) { }
		protected SessionExpiredException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }




	}
}
