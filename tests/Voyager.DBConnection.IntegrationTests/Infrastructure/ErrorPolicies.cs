using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.IntegrationTests.Infrastructure;

/// <summary>
/// SQL Server error mapping policy
/// </summary>
public class SqlServerErrorPolicy : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        return ex switch
        {
            SqlException sqlEx when sqlEx.Number == 2627 || sqlEx.Number == 2601 =>
                Error.ConflictError("Database.UniqueConstraint", "Record already exists"),

            SqlException sqlEx when sqlEx.Number == 547 =>
                Error.BusinessError("Database.ForeignKeyViolation",
                    "Referenced record does not exist or cannot be deleted due to existing references"),

            SqlException sqlEx when sqlEx.Number == 1205 =>
                Error.DatabaseError("Database.Deadlock", "Deadlock detected, operation can be retried"),

            SqlException sqlEx when sqlEx.Number == -2 =>
                Error.TimeoutError("Database.Timeout", "Database operation timed out"),

            TimeoutException =>
                Error.TimeoutError("Database.ConnectionTimeout", ex.Message),

            _ =>
                Error.DatabaseError("Database.Error", ex.Message)
        };
    }
}

/// <summary>
/// PostgreSQL error mapping policy
/// </summary>
public class PostgreSqlErrorPolicy : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        return ex switch
        {
            PostgresException pgEx when pgEx.SqlState == "23505" => // unique_violation
                Error.ConflictError("Database.UniqueConstraint", "Record already exists"),

            PostgresException pgEx when pgEx.SqlState == "23503" => // foreign_key_violation
                Error.BusinessError("Database.ForeignKeyViolation",
                    "Referenced record does not exist or cannot be deleted due to existing references"),

            PostgresException pgEx when pgEx.SqlState == "40P01" => // deadlock_detected
                Error.DatabaseError("Database.Deadlock", "Deadlock detected, operation can be retried"),

            PostgresException pgEx when pgEx.SqlState == "57014" => // query_canceled
                Error.TimeoutError("Database.Timeout", "Database operation timed out"),

            TimeoutException =>
                Error.TimeoutError("Database.ConnectionTimeout", ex.Message),

            _ =>
                Error.DatabaseError("Database.Error", ex.Message)
        };
    }
}

/// <summary>
/// MySQL error mapping policy
/// </summary>
public class MySqlErrorPolicy : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        return ex switch
        {
            MySqlException mysqlEx when mysqlEx.Number == 1062 => // ER_DUP_ENTRY
                Error.ConflictError("Database.UniqueConstraint", "Record already exists"),

            MySqlException mysqlEx when mysqlEx.Number == 1451 || mysqlEx.Number == 1452 => // ER_ROW_IS_REFERENCED, ER_NO_REFERENCED_ROW
                Error.BusinessError("Database.ForeignKeyViolation",
                    "Referenced record does not exist or cannot be deleted due to existing references"),

            MySqlException mysqlEx when mysqlEx.Number == 1213 => // ER_LOCK_DEADLOCK
                Error.DatabaseError("Database.Deadlock", "Deadlock detected, operation can be retried"),

            MySqlException mysqlEx when mysqlEx.Number == 1205 => // ER_LOCK_WAIT_TIMEOUT
                Error.TimeoutError("Database.Timeout", "Database operation timed out"),

            TimeoutException =>
                Error.TimeoutError("Database.ConnectionTimeout", ex.Message),

            _ =>
                Error.DatabaseError("Database.Error", ex.Message)
        };
    }
}

/// <summary>
/// Oracle error mapping policy
/// </summary>
public class OracleErrorPolicy : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        return ex switch
        {
            OracleException oraEx when oraEx.Number == 1 => // ORA-00001: unique constraint violated
                Error.ConflictError("Database.UniqueConstraint", "Record already exists"),

            OracleException oraEx when oraEx.Number == 2291 || oraEx.Number == 2292 => // ORA-02291/02292: integrity constraint violated
                Error.BusinessError("Database.ForeignKeyViolation",
                    "Referenced record does not exist or cannot be deleted due to existing references"),

            OracleException oraEx when oraEx.Number == 60 => // ORA-00060: deadlock detected
                Error.DatabaseError("Database.Deadlock", "Deadlock detected, operation can be retried"),

            OracleException oraEx when oraEx.Number == 1013 => // ORA-01013: user requested cancel
                Error.TimeoutError("Database.Timeout", "Database operation timed out"),

            TimeoutException =>
                Error.TimeoutError("Database.ConnectionTimeout", ex.Message),

            _ =>
                Error.DatabaseError("Database.Error", ex.Message)
        };
    }
}
