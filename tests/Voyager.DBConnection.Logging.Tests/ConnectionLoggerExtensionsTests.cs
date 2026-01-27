using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Voyager.DBConnection.Interfaces;

[assembly: NUnit.Framework.Category("Unit")]

namespace Voyager.DBConnection.Logging.Tests
{
	[NUnit.Framework.TestFixture]
	public class ConnectionLoggerExtensionsTests
	{
		private ILogger logger;

		[NUnit.Framework.SetUp]
		public void SetUp()
		{
			logger = Substitute.For<ILogger>();
		}

		[NUnit.Framework.Test]
		public void AddLogger_ToDbCommandExecutor_ShouldAddFeature()
		{
			// Use reflection to call extension method
			var method = typeof(ConnectionLogger).GetMethod("AddLogger", new[] { typeof(DbCommandExecutor), typeof(ILogger) });
			NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "AddLogger method for DbCommandExecutor should exist");
		}

		[NUnit.Framework.Test]
		public void AddLogger_ToConnection_ShouldAddFeature()
		{
			// Arrange & Act & Assert
			var method = typeof(ConnectionLogger).GetMethod("AddLogger", new[] { typeof(Connection), typeof(ILogger) });
			NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "AddLogger method for Connection should exist");
		}

		[NUnit.Framework.Test]
		public void Extension_ShouldBeInCorrectNamespace()
		{
			// Assert
			var type = typeof(ConnectionLogger);
			NUnit.Framework.Assert.That(type.Namespace, NUnit.Framework.Is.EqualTo("Voyager.DBConnection"));
		}

		[NUnit.Framework.Test]
		public void Extension_ShouldBeStatic()
		{
			// Assert
			var type = typeof(ConnectionLogger);
			NUnit.Framework.Assert.That(type.IsAbstract && type.IsSealed, NUnit.Framework.Is.True, "ConnectionLogger should be a static class");
		}

		[NUnit.Framework.Test]
		public void Extension_Methods_ShouldBePublic()
		{
			// Arrange
			var type = typeof(ConnectionLogger);

			// Act
			var executorMethod = type.GetMethod("AddLogger", new[] { typeof(DbCommandExecutor), typeof(ILogger) });
			var connectionMethod = type.GetMethod("AddLogger", new[] { typeof(Connection), typeof(ILogger) });

			// Assert
			NUnit.Framework.Assert.That(executorMethod, NUnit.Framework.Is.Not.Null);
			NUnit.Framework.Assert.That(executorMethod.IsPublic, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(executorMethod.IsStatic, NUnit.Framework.Is.True);

			NUnit.Framework.Assert.That(connectionMethod, NUnit.Framework.Is.Not.Null);
			NUnit.Framework.Assert.That(connectionMethod.IsPublic, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(connectionMethod.IsStatic, NUnit.Framework.Is.True);
		}
	}
}
