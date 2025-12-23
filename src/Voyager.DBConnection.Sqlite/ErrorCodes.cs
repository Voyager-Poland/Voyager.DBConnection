namespace Voyager.DBConnection.Sqlite
{
	internal static class ErrorCodes
	{
		// SQLite result codes
		public const int SQLITE_CONSTRAINT = 19; // Constraint violation
		public const int SQLITE_BUSY = 5; // Database is locked
		public const int SQLITE_LOCKED = 6; // Database table is locked
		public const int SQLITE_IOERR = 10; // Disk I/O error
		public const int SQLITE_CORRUPT = 11; // Database disk image is malformed
	}
}
