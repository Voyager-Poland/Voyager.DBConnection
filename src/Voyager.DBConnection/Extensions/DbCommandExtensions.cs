using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection
{
    /// <summary>
    /// Fluent API extension methods for DbCommand parameter management.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds an input parameter to the command.
        /// </summary>
        public static DbCommand WithInputParameter(this DbCommand command, string name, DbType dbType, object value)
            => command.AddParameter(name, dbType, 0, ParameterDirection.Input, value);

        /// <summary>
        /// Adds an input parameter to the command with specified size.
        /// </summary>
        public static DbCommand WithInputParameter(this DbCommand command, string name, DbType dbType, int size, object value)
            => command.AddParameter(name, dbType, size, ParameterDirection.Input, value);

        /// <summary>
        /// Adds an output parameter to the command.
        /// </summary>
        public static DbCommand WithOutputParameter(this DbCommand command, string name, DbType dbType, int size)
            => command.AddParameter(name, dbType, size, ParameterDirection.Output, DBNull.Value);

        /// <summary>
        /// Adds an input/output parameter to the command.
        /// </summary>
        public static DbCommand WithInputOutputParameter(this DbCommand command, string name, DbType dbType, object value)
            => command.AddParameter(name, dbType, 0, ParameterDirection.InputOutput, value);

        /// <summary>
        /// Adds an input/output parameter to the command with specified size.
        /// </summary>
        public static DbCommand WithInputOutputParameter(this DbCommand command, string name, DbType dbType, int size, object value)
            => command.AddParameter(name, dbType, size, ParameterDirection.InputOutput, value);

        /// <summary>
        /// Adds a parameter to the command with full configuration.
        /// </summary>
        public static DbCommand WithParameter(this DbCommand command, string name, DbType dbType, int size, ParameterDirection direction, object value)
            => command.AddParameter(name, dbType, size, direction, value);

        /// <summary>
        /// Gets the value of a parameter by name.
        /// </summary>
        public static object GetParameterValue(this DbCommand command, string name)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var paramName = BuildParameterName(command, name);
            return command.Parameters[paramName].Value;
        }

        /// <summary>
        /// Gets the value of a parameter by name, cast to the specified type.
        /// </summary>
        public static T GetParameterValue<T>(this DbCommand command, string name)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var paramName = BuildParameterName(command, name);
            var value = command.Parameters[paramName].Value;
            if (value == DBNull.Value || value == null)
                return default;

            return (T)value;
        }

        private static DbCommand AddParameter(this DbCommand command, string name, DbType dbType, int size, ParameterDirection direction, object value)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var param = command.CreateParameter();
            param.ParameterName = BuildParameterName(command, name);
            param.DbType = dbType;
            if (size > 0)
                param.Size = size;
            param.Direction = direction;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
            return command;
        }

        private static string BuildParameterName(DbCommand command, string name)
        {
            var prefix = GetParameterPrefix(command);

            // Jeśli nazwa już ma odpowiedni prefix, nie dodawaj
            if (!string.IsNullOrEmpty(prefix) && !name.StartsWith(prefix))
                return prefix + name;

            return name;
        }

        private static string GetParameterPrefix(DbCommand command)
        {
            var typeName = command.GetType().FullName ?? "";

            // Oracle używa :
            if (typeName.Contains("Oracle"))
                return ":";

            // MS SQL, PostgreSQL, MySQL, SQLite używają @
            if (typeName.Contains("Sql") || typeName.Contains("Npgsql") || typeName.Contains("MySql") || typeName.Contains("SQLite"))
                return "@";

            // Nieznany provider - brak prefixu
            return "";
        }
    }
}
