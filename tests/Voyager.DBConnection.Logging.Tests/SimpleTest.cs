using NUnit.Framework;

namespace Voyager.DBConnection.Logging.Tests
{
	[TestFixture]
	public class SimpleTest
	{
		[Test]
		public void ExtensionClass_ShouldExist()
		{
			// Arrange & Act
			var type = typeof(ConnectionLogger);

			// Assert
			Assert.That(type, Is.Not.Null);
			Assert.That(type.IsClass, Is.True);
		}

		[Test]
		public void ExtensionMethods_ShouldBeAvailable()
		{
			// Arrange
			var type = typeof(ConnectionLogger);

			// Act
			var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			// Assert
			Assert.That(methods.Length, Is.GreaterThanOrEqualTo(2), "Should have at least 2 AddLogger extension methods");
		}
	}
}
