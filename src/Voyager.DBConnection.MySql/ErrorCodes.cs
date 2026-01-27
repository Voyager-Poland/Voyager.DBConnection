namespace Voyager.DBConnection.MySql
{
	internal static class ErrorCodes
	{
		// MySQL error codes
		public const int DuplicateEntry = 1062;
		public const int LockWaitTimeout = 1205;
		public const int DeadlockFound = 1213;
		public const int ForeignKeyConstraintFails = 1452;
		public const int ConstraintFails = 1216;
	}
}
