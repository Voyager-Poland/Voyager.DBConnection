using Voyager.DBConnection.Exceptions;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
    internal static class ParameterValidator
    {
        public static void DbGuard(Database db)
        {
            if (db == null)
                throw new NoDbException();
        }

        public static void DBPolicyGuard(IExceptionPolicy exceptionPolicy)
        {
            if (exceptionPolicy == null)
                throw new LackExceptionPolicyException();
        }

        public static void DBPolicyGuard(IMapErrorPolicy exceptionPolicy)
        {
            if (exceptionPolicy == null)
                throw new LackExceptionPolicyException();
        }
    }
}
