namespace Voyager.DBConnection.Oracle
{
	internal static class ErrorCodes
	{
		// Oracle error codes
		public const int UniqueConstraintViolation = 1; // ORA-00001
		public const int Timeout = 2049; // ORA-02049: timeout: distributed transaction waiting for lock
		public const int DeadlockDetected = 60; // ORA-00060: deadlock detected while waiting for resource
		public const int ForeignKeyViolation = 2291; // ORA-02291: integrity constraint violated - parent key not found
		public const int ConstraintViolation = 2290; // ORA-02290: check constraint violated
	}
}
