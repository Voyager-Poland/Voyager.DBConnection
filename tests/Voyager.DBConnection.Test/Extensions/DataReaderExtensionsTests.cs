using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Voyager.DBConnection.Test.Extensions
{
	[TestFixture]
	public class DataReaderExtensionsTests
	{
		private Mock<IDataReader> mockReader;

		[SetUp]
		public void SetUp()
		{
			mockReader = new Mock<IDataReader>();
		}

		#region FetchAll Tests

		[Test]
		public void FetchAll_WithMultipleRows_ShouldReturnArray()
		{
			// Arrange
			var readSequence = new Queue<bool>(new[] { true, true, true, false });
			var idSequence = new Queue<int>(new[] { 1, 2, 3 });
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(() => idSequence.Dequeue());

			// Act
			var result = mockReader.Object.FetchAll(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Length, Is.EqualTo(3));
			Assert.That(result, Is.InstanceOf<TestUser[]>());
		}

		[Test]
		public void FetchAll_WithNoRows_ShouldReturnEmptyArray()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object.FetchAll(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Length, Is.EqualTo(0));
		}

		[Test]
		public void FetchAll_WithNullDataReader_ShouldThrowArgumentNullException()
		{
			// Arrange
			IDataReader nullReader = null;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				nullReader.FetchAll(r => new TestUser()));
		}

		[Test]
		public void FetchAll_WithNullCreateItem_ShouldThrowArgumentNullException()
		{
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				mockReader.Object.FetchAll<TestUser>(null));
		}

		#endregion

		#region FetchList Tests

		[Test]
		public void FetchList_WithMultipleRows_ShouldReturnList()
		{
			// Arrange
			var readSequence = new Queue<bool>(new[] { true, true, false });
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(1);

			// Act
			var result = mockReader.Object.FetchList(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result, Is.InstanceOf<List<TestUser>>());
		}

		[Test]
		public void FetchList_WithNoRows_ShouldReturnEmptyList()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object.FetchList(r => new TestUser());

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		#endregion

		#region FetchFirstOrDefault Tests

		[Test]
		public void FetchFirstOrDefault_WithRows_ShouldReturnFirstRow()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(true);
			mockReader.Setup(r => r.GetInt32(0)).Returns(42);
			mockReader.Setup(r => r.GetString(1)).Returns("John");

			// Act
			var result = mockReader.Object.FetchFirstOrDefault(r => new TestUser
			{
				Id = r.GetInt32(0),
				Name = r.GetString(1)
			});

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(42));
			Assert.That(result.Name, Is.EqualTo("John"));
		}

		[Test]
		public void FetchFirstOrDefault_WithNoRows_ShouldReturnDefault()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object.FetchFirstOrDefault(r => new TestUser());

			// Assert
			Assert.That(result, Is.Null);
		}

		[Test]
		public void FetchFirstOrDefault_WithValueType_ShouldReturnDefaultValue()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object.FetchFirstOrDefault(r => r.GetInt32(0));

			// Assert
			Assert.That(result, Is.EqualTo(0));
		}

		#endregion

		#region FetchFirst Tests

		[Test]
		public void FetchFirst_WithRows_ShouldReturnFirstRow()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(true);
			mockReader.Setup(r => r.GetInt32(0)).Returns(100);

			// Act
			var result = mockReader.Object.FetchFirst(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(100));
		}

		[Test]
		public void FetchFirst_WithNoRows_ShouldThrowInvalidOperationException()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() =>
				mockReader.Object.FetchFirst(r => new TestUser()));

			Assert.That(ex.Message, Is.EqualTo("The data reader contains no rows."));
		}

		#endregion

		#region FetchSingle Tests

		[Test]
		public void FetchSingle_WithSingleRow_ShouldReturnRow()
		{
			// Arrange
			var readSequence = new Queue<bool>(new[] { true, false });
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(99);

			// Act
			var result = mockReader.Object.FetchSingle(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(99));
		}

		[Test]
		public void FetchSingle_WithNoRows_ShouldThrowInvalidOperationException()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() =>
				mockReader.Object.FetchSingle(r => new TestUser()));

			Assert.That(ex.Message, Is.EqualTo("The data reader contains no rows."));
		}

		[Test]
		public void FetchSingle_WithMultipleRows_ShouldThrowInvalidOperationException()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(true);
			mockReader.Setup(r => r.GetInt32(0)).Returns(1);

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() =>
				mockReader.Object.FetchSingle(r => new TestUser { Id = r.GetInt32(0) }));

			Assert.That(ex.Message, Is.EqualTo("The data reader contains more than one row."));
		}

		#endregion

		#region FetchSingleOrDefault Tests

		[Test]
		public void FetchSingleOrDefault_WithSingleRow_ShouldReturnRow()
		{
			// Arrange
			var readSequence = new Queue<bool>(new[] { true, false });
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(77);

			// Act
			var result = mockReader.Object.FetchSingleOrDefault(r => new TestUser { Id = r.GetInt32(0) });

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(77));
		}

		[Test]
		public void FetchSingleOrDefault_WithNoRows_ShouldReturnDefault()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object.FetchSingleOrDefault(r => new TestUser());

			// Assert
			Assert.That(result, Is.Null);
		}

		[Test]
		public void FetchSingleOrDefault_WithMultipleRows_ShouldThrowInvalidOperationException()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(true);

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() =>
				mockReader.Object.FetchSingleOrDefault(r => new TestUser()));

			Assert.That(ex.Message, Is.EqualTo("The data reader contains more than one row."));
		}

		#endregion

		#region AsEnumerable Tests

		[Test]
		public void AsEnumerable_WithMultipleRows_ShouldReturnEnumerable()
		{
			// Arrange
			var ids = new Queue<int>(new[] { 1, 2, 3 });
			var names = new Queue<string>(new[] { "Alice", "Bob", "Charlie" });
			var readSequence = new Queue<bool>(new[] { true, true, true, false });
			
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(() => ids.Dequeue());
			mockReader.Setup(r => r.GetString(1)).Returns(() => names.Dequeue());

			// Act
			var result = mockReader.Object.AsEnumerable(r => new TestUser
			{
				Id = r.GetInt32(0),
				Name = r.GetString(1)
			});

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.InstanceOf<IEnumerable<TestUser>>());
			
			var list = result.ToList();
			Assert.That(list.Count, Is.EqualTo(3));
			Assert.That(list[0].Name, Is.EqualTo("Alice"));
			Assert.That(list[1].Name, Is.EqualTo("Bob"));
			Assert.That(list[2].Name, Is.EqualTo("Charlie"));
		}

		[Test]
		public void AsEnumerable_WithLINQWhere_ShouldFilterCorrectly()
		{
			// Arrange
			var ids = new Queue<int>(new[] { 1, 2, 3, 4, 5 });
			var readSequence = new Queue<bool>(new[] { true, true, true, true, true, false });
			
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(() => ids.Dequeue());

			// Act
			var result = mockReader.Object
				.AsEnumerable(r => new TestUser { Id = r.GetInt32(0) })
				.Where(u => u.Id > 2)
				.ToList();

			// Assert
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].Id, Is.EqualTo(3));
			Assert.That(result[1].Id, Is.EqualTo(4));
			Assert.That(result[2].Id, Is.EqualTo(5));
		}

		[Test]
		public void AsEnumerable_WithLINQTake_ShouldLimitResults()
		{
			// Arrange
			var ids = new Queue<int>(new[] { 1, 2, 3, 4, 5 });
			var readSequence = new Queue<bool>(new[] { true, true, true, true, true, false });
			
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(() => ids.Dequeue());

			// Act
			var result = mockReader.Object
				.AsEnumerable(r => new TestUser { Id = r.GetInt32(0) })
				.Take(3)
				.ToList();

			// Assert
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].Id, Is.EqualTo(1));
			Assert.That(result[2].Id, Is.EqualTo(3));
		}

		[Test]
		public void AsEnumerable_WithLINQSelect_ShouldProjectCorrectly()
		{
			// Arrange
			var ids = new Queue<int>(new[] { 1, 2, 3 });
			var names = new Queue<string>(new[] { "Alice", "Bob", "Charlie" });
			var readSequence = new Queue<bool>(new[] { true, true, true, false });
			
			mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
			mockReader.Setup(r => r.GetInt32(0)).Returns(() => ids.Dequeue());
			mockReader.Setup(r => r.GetString(1)).Returns(() => names.Dequeue());

			// Act
			var result = mockReader.Object
				.AsEnumerable(r => new TestUser { Id = r.GetInt32(0), Name = r.GetString(1) })
				.Select(u => u.Name.ToUpper())
				.ToList();

			// Assert
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0], Is.EqualTo("ALICE"));
			Assert.That(result[1], Is.EqualTo("BOB"));
			Assert.That(result[2], Is.EqualTo("CHARLIE"));
		}

		[Test]
		public void AsEnumerable_WithNoRows_ShouldReturnEmptyEnumerable()
		{
			// Arrange
			mockReader.Setup(r => r.Read()).Returns(false);

			// Act
			var result = mockReader.Object
				.AsEnumerable(r => new TestUser { Id = r.GetInt32(0) })
				.ToList();

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[Test]
		public void AsEnumerable_WithNullDataReader_ShouldThrowArgumentNullException()
		{
			// Arrange
			IDataReader nullReader = null;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				nullReader.AsEnumerable(r => new TestUser()));
		}

		[Test]
		public void AsEnumerable_WithNullCreateItem_ShouldThrowArgumentNullException()
		{
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				mockReader.Object.AsEnumerable<TestUser>(null));
		}

		#endregion

		// Test helper class
		private class TestUser
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
		}
	}
}
