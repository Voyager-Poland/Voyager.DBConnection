using System;
using System.Data;
using System.Data.Common;
using Moq;
using NUnit.Framework;
using Voyager.Common.Results;
using Voyager.DBConnection;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.Internal;

namespace Voyager.DBConnection.Test
{
	[TestFixture]
	public class DbCommandExecutorUnitTests
	{
		private Mock<IDatabaseInternal> mockDatabase;
		private Mock<IMapErrorPolicy> mockErrorPolicy;
		private Mock<ICommandFactoryHelper> mockCommandFactory;
		private Mock<IErrorMappingHelper> mockErrorMapper;
		private Mock<ICommandExecutionHelper> mockCommandExecution;
		private Mock<IDataReaderHelper> mockDataReader;
		private EventHost eventHost;
		private FeatureHost featureHost;

		[SetUp]
		public void SetUp()
		{
			mockDatabase = new Mock<IDatabaseInternal>();
			mockErrorPolicy = new Mock<IMapErrorPolicy>();
			mockCommandFactory = new Mock<ICommandFactoryHelper>();
			mockErrorMapper = new Mock<IErrorMappingHelper>();
			mockCommandExecution = new Mock<ICommandExecutionHelper>();
			mockDataReader = new Mock<IDataReaderHelper>();
			eventHost = new EventHost();
			featureHost = new FeatureHost();
		}

		[TearDown]
		public void TearDown()
		{
			featureHost?.Dispose();
		}

		private DbCommandExecutor CreateExecutor()
		{
			return new DbCommandExecutor(
				mockDatabase.Object,
				mockErrorPolicy.Object,
				mockCommandFactory.Object,
				mockErrorMapper.Object,
				mockCommandExecution.Object,
				mockDataReader.Object,
				eventHost,
				featureHost
			);
		}

		[Test]
		public void BeginTransaction_ShouldCallDatabaseBeginTransaction()
		{
			// Arrange
			var mockTransactionHolder = new TransactionHolder(new TestDbConnection(), IsolationLevel.ReadCommitted);
			var mockTransaction = new Transaction(mockTransactionHolder, () => { });

			mockDatabase.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>()))
				.Returns(mockTransaction);

			var executor = CreateExecutor();

			// Act
			var transaction = executor.BeginTransaction();

			// Assert
			mockDatabase.Verify(x => x.BeginTransaction(IsolationLevel.ReadCommitted), Times.Once);
			Assert.That(transaction, Is.Not.Null);
		}

		[Test]
		public void ExecuteNonQuery_WithProcedureName_ShouldUseCommandFactoryHelper()
		{
			// Arrange
			var mockCommand = new TestDbCommand();
			var commandFactory = new Func<DbCommand>(() => mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommandFactory("TestProcedure"))
				.Returns(commandFactory);

			Result<int> result1 = 1; // implicit conversion
			mockCommandExecution.Setup(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, int>>()))
				.Returns(result1);

			var executor = CreateExecutor();

			// Act
			var result = executor.ExecuteNonQuery("TestProcedure");

			// Assert
			mockCommandFactory.Verify(x => x.CreateCommandFactory("TestProcedure"), Times.Once);
			mockCommandExecution.Verify(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, int>>()), Times.Once);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Value, Is.EqualTo(1));
		}

		[Test]
		public void ExecuteNonQuery_WithCommandFunction_ShouldUseCommandFactoryHelper()
		{
			// Arrange
			var mockCommand = new TestDbCommand();
			Func<IDatabase, DbCommand> commandFunction = db => mockCommand;
			var commandFactory = new Func<DbCommand>(() => mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommandFactory(commandFunction))
				.Returns(commandFactory);

			Result<int> result5 = 5; // implicit conversion
			mockCommandExecution.Setup(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, int>>()))
				.Returns(result5);

			var executor = CreateExecutor();

			// Act
			var result = executor.ExecuteNonQuery(commandFunction);

			// Assert
			mockCommandFactory.Verify(x => x.CreateCommandFactory(commandFunction), Times.Once);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Value, Is.EqualTo(5));
		}

		[Test]
		public void ExecuteScalar_WithProcedureName_ShouldReturnScalarValue()
		{
			// Arrange
			var mockCommand = new TestDbCommand();
			var commandFactory = new Func<DbCommand>(() => mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommandFactory("GetCount"))
				.Returns(commandFactory);

			Result<object> result42 = 42; // implicit conversion
			mockCommandExecution.Setup(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, object>>()))
				.Returns(result42);

			var executor = CreateExecutor();

			// Act
			var result = executor.ExecuteScalar("GetCount");

			// Assert
			mockCommandFactory.Verify(x => x.CreateCommandFactory("GetCount"), Times.Once);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Value, Is.EqualTo(42));
		}


		[Test]
		public void Dispose_ShouldDisposeDatabaseAndFeatureHost()
		{
			// Arrange
			var executor = CreateExecutor();

			// Act
			executor.Dispose();

			// Assert
			mockDatabase.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void AddFeature_ShouldAllowFeatureAddition()
		{
			// Arrange
			var executor = CreateExecutor();
			var mockFeature = new Mock<IFeature>();

			// Act & Assert - Should not throw
			Assert.DoesNotThrow(() => executor.AddFeature(mockFeature.Object));
		}

		[Test]
		public void ExecuteNonQuery_WithIDbCommandFactory_ShouldUseCommandFactoryHelper()
		{
			// Arrange
			var mockCommand = new TestDbCommand();
			var mockCommandFactoryInterface = new Mock<IDbCommandFactory>();

			var commandFactory = new Func<DbCommand>(() => mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommand(mockCommandFactoryInterface.Object))
				.Returns(mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommandFactory(mockCommandFactoryInterface.Object))
				.Returns(commandFactory);

			Result<int> result3 = 3; // implicit conversion
			mockCommandExecution.Setup(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, int>>()))
				.Returns(result3);

			var executor = CreateExecutor();

			// Act
			var result = executor.ExecuteNonQuery(mockCommandFactoryInterface.Object);

			// Assert
			mockCommandFactory.Verify(x => x.CreateCommandFactory(mockCommandFactoryInterface.Object), Times.Once);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Value, Is.EqualTo(3));
		}

		[Test]
		public void ExecuteScalar_WithCommandFunction_ShouldReturnScalarValue()
		{
			// Arrange
			var mockCommand = new TestDbCommand();
			Func<IDatabase, DbCommand> commandFunction = db => mockCommand;
			var commandFactory = new Func<DbCommand>(() => mockCommand);

			mockCommandFactory.Setup(x => x.CreateCommandFactory(commandFunction))
				.Returns(commandFactory);

			Result<object> resultStr = "test_value"; // implicit conversion
			mockCommandExecution.Setup(x => x.ExecuteWithEvents(
				It.IsAny<DbCommand>(),
				It.IsAny<Func<DbCommand, object>>()))
				.Returns(resultStr);

			var executor = CreateExecutor();

			// Act
			var result = executor.ExecuteScalar(commandFunction);

			// Assert
			mockCommandFactory.Verify(x => x.CreateCommandFactory(commandFunction), Times.Once);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Value, Is.EqualTo("test_value"));
		}
	}
}
