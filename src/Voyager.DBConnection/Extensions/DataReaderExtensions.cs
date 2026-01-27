using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Voyager.DBConnection
{
	/// <summary>
	/// Extension methods for <see cref="IDataReader"/> to simplify data extraction and mapping.
	/// Provides LINQ-like methods (First, Single, etc.) for working with data readers.
	/// </summary>
	/// <remarks>
	/// These extension methods follow familiar LINQ patterns but are optimized for IDataReader.
	/// All methods handle null checks and provide clear exception messages.
	/// Use these methods within IResultsConsumer implementations or afterCall callbacks.
	/// </remarks>
	public static class DataReaderExtensions
	{
		/// <summary>
		/// Converts an IDataReader into an enumerable sequence that can be used with LINQ.
		/// </summary>
		/// <typeparam name="TValue">The type of objects to create from each row.</typeparam>
		/// <param name="dataReader">The data reader to enumerate. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>An enumerable sequence of objects created from the data reader rows.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <remarks>
		/// This method enables LINQ operations on IDataReader without loading all data into memory first.
		/// The enumeration is lazy - rows are read on-demand as you iterate.
		/// Use this with LINQ methods like Where, Select, Take, Skip, etc.
		/// Warning: Some LINQ operations (OrderBy, Count) will enumerate the entire sequence.
		/// </remarks>
		/// <example>
		/// Using LINQ with AsEnumerable:
		/// <code>
		/// var activeUsers = reader
		///     .AsEnumerable(r => new User 
		///     {
		///         Id = r.GetInt32(0),
		///         Name = r.GetString(1),
		///         IsActive = r.GetBoolean(2)
		///     })
		///     .Where(u => u.IsActive)
		///     .Take(10)
		///     .ToList();
		/// </code>
		/// </example>
		public static IEnumerable<TValue> AsEnumerable<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			return new DataReaderEnumerable<TValue>(dataReader, createItem);
		}

		/// <summary>
		/// Reads all rows from the data reader and transforms them into an array using the provided factory function.
		/// </summary>
		/// <typeparam name="TValue">The type of objects to create from each row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>An array containing all objects created from the data reader rows. Returns empty array if no rows.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <remarks>
		/// This method reads all available rows from the current result set.
		/// If you need multiple result sets, call NextResult() on the reader between calls.
		/// The returned array is a snapshot - modifications won't affect the source data.
		/// </remarks>
		/// <example>
		/// Using with ExecuteReader:
		/// <code>
		/// var users = executor.ExecuteReader(
		///     db => db.GetSqlCommand("SELECT UserId, Username, Email FROM Users"),
		///     reader => reader.FetchAll(r => new User 
		///     {
		///         Id = r.GetInt32(r.GetOrdinal("UserId")),
		///         Name = r.GetString(r.GetOrdinal("Username")),
		///         Email = r.GetString(r.GetOrdinal("Email"))
		///     })
		/// );
		/// </code>
		/// </example>
		public static TValue[] FetchAll<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			var items = new List<TValue>();
			while (dataReader.Read())
			{
				items.Add(createItem(dataReader));
			}
			return items.ToArray();
		}

		/// <summary>
		/// Reads all rows from the data reader and transforms them into a list using the provided factory function.
		/// </summary>
		/// <typeparam name="TValue">The type of objects to create from each row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>A mutable list containing all objects created from the data reader rows. Returns empty list if no rows.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <remarks>
		/// Use this instead of FetchAll when you need to modify the collection after retrieval.
		/// The returned List allows adding, removing, or sorting items.
		/// For read-only collections, prefer FetchAll which returns an array.
		/// </remarks>
		/// <example>
		/// Using with post-processing:
		/// <code>
		/// var users = executor.ExecuteReader(
		///     "GetActiveUsers",
		///     reader => reader.FetchList(r => new User 
		///     {
		///         Id = r.GetInt32(0),
		///         Name = r.GetString(1)
		///     })
		/// );
		/// 
		/// // List allows modification
		/// users.Add(new User { Id = 0, Name = "Guest" });
		/// users.Sort((a, b) => a.Name.CompareTo(b.Name));
		/// </code>
		/// </example>
		public static List<TValue> FetchList<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			var items = new List<TValue>();
			while (dataReader.Read())
			{
				items.Add(createItem(dataReader));
			}
			return items;
		}

		/// <summary>
		/// Reads the first row from the data reader and transforms it into an object using the provided factory function.
		/// Returns the default value if no rows are available.
		/// </summary>
		/// <typeparam name="TValue">The type of object to create from the row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>The created object from the first row, or <c>default(TValue)</c> (null for reference types, 0 for value types) if no rows.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <remarks>
		/// This method only reads the first row. Additional rows are ignored (not consumed).
		/// Useful for queries that may return zero or one row (e.g., optional lookups).
		/// For reference types, returns null if no rows. For value types, returns default value (0, false, etc.).
		/// </remarks>
		/// <example>
		/// Safe lookup that handles missing data:
		/// <code>
		/// var config = executor.ExecuteReader(
		///     "GetConfigValue",
		///     cmd => cmd.WithInputParameter("key", DbType.String, "MaxRetries"),
		///     reader => reader.FetchFirstOrDefault(r => r.GetString(0))
		/// );
		/// 
		/// // config is null if key doesn't exist
		/// var maxRetries = int.Parse(config ?? "3");
		/// </code>
		/// </example>
		public static TValue FetchFirstOrDefault<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			if (dataReader.Read())
			{
				return createItem(dataReader);
			}
			return default(TValue);
		}

		/// <summary>
		/// Reads the first row from the data reader and transforms it into an object using the provided factory function.
		/// Throws an exception if no rows are available.
		/// </summary>
		/// <typeparam name="TValue">The type of object to create from the row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>The created object from the first row.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the data reader contains no rows.</exception>
		/// <remarks>
		/// Use this when you expect at least one row and want to fail fast if data is missing.
		/// This method only reads the first row. Additional rows are ignored.
		/// If you need to verify there's exactly one row, use FetchSingle instead.
		/// </remarks>
		/// <example>
		/// Required lookup that must succeed:
		/// <code>
		/// var currentUser = executor.ExecuteReader(
		///     "GetCurrentUser",
		///     reader => reader.FetchFirst(r => new User 
		///     {
		///         Id = r.GetInt32(r.GetOrdinal("UserId")),
		///         Name = r.GetString(r.GetOrdinal("Username"))
		///     })
		/// );
		/// // Throws InvalidOperationException if no user found
		/// </code>
		/// </example>
		public static TValue FetchFirst<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			if (dataReader.Read())
			{
				return createItem(dataReader);
			}
			throw new InvalidOperationException("The data reader contains no rows.");
		}

		/// <summary>
		/// Reads a single row from the data reader and transforms it into an object using the provided factory function.
		/// Throws an exception if there are no rows or more than one row.
		/// </summary>
		/// <typeparam name="TValue">The type of object to create from the row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>The created object from the single row.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the data reader contains no rows or more than one row.</exception>
		/// <remarks>
		/// Use this when you expect EXACTLY one row and want to enforce this invariant.
		/// Common use cases: primary key lookups, aggregate queries, singleton configurations.
		/// This method validates both conditions: (1) at least one row exists, (2) no more than one row exists.
		/// If the query might return 0 rows, use FetchSingleOrDefault instead.
		/// </remarks>
		/// <example>
		/// Primary key lookup that enforces uniqueness:
		/// <code>
		/// var user = executor.ExecuteReader(
		///     "GetUserById",
		///     cmd => cmd.WithInputParameter("userId", DbType.Int32, 42),
		///     reader => reader.FetchSingle(r => new User 
		///     {
		///         Id = r.GetInt32(0),
		///         Name = r.GetString(1),
		///         Email = r.GetString(2)
		///     })
		/// );
		/// // Throws if user doesn't exist OR if duplicate IDs exist (data integrity issue)
		/// </code>
		/// </example>
		public static TValue FetchSingle<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			if (!dataReader.Read())
			{
				throw new InvalidOperationException("The data reader contains no rows.");
			}

			var result = createItem(dataReader);

			if (dataReader.Read())
			{
				throw new InvalidOperationException("The data reader contains more than one row.");
			}

			return result;
		}

		/// <summary>
		/// Reads a single row from the data reader and transforms it into an object using the provided factory function.
		/// Returns the default value if no rows are available. Throws an exception if there is more than one row.
		/// </summary>
		/// <typeparam name="TValue">The type of object to create from the row.</typeparam>
		/// <param name="dataReader">The data reader to read from. Must not be null.</param>
		/// <param name="createItem">A function that creates an object from the current row. Must not be null.</param>
		/// <returns>The created object from the single row, or <c>default(TValue)</c> if no rows are available.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> or <paramref name="createItem"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the data reader contains more than one row.</exception>
		/// <remarks>
		/// Use this when you expect AT MOST one row and want to enforce uniqueness.
		/// Common use cases: optional lookups by unique key, "get current" scenarios.
		/// Unlike FetchSingle, this allows zero rows but still validates there's no more than one.
		/// The "Or Default" naming follows LINQ conventions (returns null/0 for no rows).
		/// </remarks>
		/// <example>
		/// Optional unique lookup:
		/// <code>
		/// var setting = executor.ExecuteReader(
		///     "GetUserPreference",
		///     cmd => cmd
		///         .WithInputParameter("userId", DbType.Int32, currentUserId)
		///         .WithInputParameter("key", DbType.String, "Theme"),
		///     reader => reader.FetchSingleOrDefault(r => r.GetString(0))
		/// );
		/// 
		/// var theme = setting ?? "Dark"; // Use default if user hasn't set preference
		/// // Throws InvalidOperationException if database has duplicate preferences (data corruption)
		/// </code>
		/// </example>
		public static TValue FetchSingleOrDefault<TValue>(this IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			if (dataReader == null) throw new ArgumentNullException(nameof(dataReader));
			if (createItem == null) throw new ArgumentNullException(nameof(createItem));

			if (!dataReader.Read())
			{
				return default(TValue);
			}

			var result = createItem(dataReader);

			if (dataReader.Read())
			{
				throw new InvalidOperationException("The data reader contains more than one row.");
			}

			return result;
		}
	}

	/// <summary>
	/// Provides an enumerable wrapper around IDataReader for LINQ support.
	/// </summary>
	/// <typeparam name="TValue">The type of objects created from each row.</typeparam>
	internal class DataReaderEnumerable<TValue> : IEnumerable<TValue>
	{
		private readonly IDataReader dataReader;
		private readonly Func<IDataReader, TValue> createItem;

		public DataReaderEnumerable(IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			this.dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
			this.createItem = createItem ?? throw new ArgumentNullException(nameof(createItem));
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return new DataReaderEnumerator<TValue>(dataReader, createItem);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Provides an enumerator over IDataReader rows.
	/// </summary>
	/// <typeparam name="TValue">The type of objects created from each row.</typeparam>
	internal class DataReaderEnumerator<TValue> : IEnumerator<TValue>
	{
		private readonly IDataReader dataReader;
		private readonly Func<IDataReader, TValue> createItem;
		private TValue current;
		private bool disposed;

		public DataReaderEnumerator(IDataReader dataReader, Func<IDataReader, TValue> createItem)
		{
			this.dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
			this.createItem = createItem ?? throw new ArgumentNullException(nameof(createItem));
		}

		public TValue Current
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException(nameof(DataReaderEnumerator<TValue>));
				return current;
			}
		}

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(DataReaderEnumerator<TValue>));

			if (dataReader.Read())
			{
				current = createItem(dataReader);
				return true;
			}

			current = default(TValue);
			return false;
		}

		public void Reset()
		{
			throw new NotSupportedException("IDataReader does not support Reset operation.");
		}

		public void Dispose()
		{
			if (!disposed)
			{
				// Note: We don't dispose the dataReader here because it's owned by the caller
				// The caller is responsible for disposing the reader
				current = default(TValue);
				disposed = true;
			}
		}
	}
}
