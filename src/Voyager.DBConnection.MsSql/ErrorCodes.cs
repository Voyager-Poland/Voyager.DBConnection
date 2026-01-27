namespace Voyager.DBConnection.MsSql
{
	internal static class ErrorCodes
	{
		public const byte ValidationError = 1;
		public const byte ConcurrencyViolationError = 2;
		public const byte PrivilageNameValidation = 3;
		public const byte ParamVeryfication = 4;
		public const int SqlUserRaisedError = 50000;
		public const int DeadLockNumber = 1205;

		public const int SqlUniqueConstraintViolation = 2627;

		public const int Timeout_adonetNumber = -2;
		public const int ConnectionNumber = 4060;
		public const int Readony = 3906;
		public const int SessionExpired = 50003;
	}
}
