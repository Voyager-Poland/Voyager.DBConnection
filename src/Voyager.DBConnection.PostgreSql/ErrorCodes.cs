namespace Voyager.DBConnection.PostgreSql
{
	internal static class ErrorCodes
	{
		// PostgreSQL error codes (SQLSTATE)
		public const string UniqueViolation = "23505";
		public const string ForeignKeyViolation = "23503";
		public const string QueryCanceled = "57014";
		public const string DeadlockDetected = "40P01";
		public const string SerializationFailure = "40001";
	}
}
