using Voyager.DBConnection;

namespace Voyager.Tests
{
	[TestFixture]
	public class ParamNameRuleTests
	{
		[Test]
		public void GetParamName_WhenParamNameDoesNotStartWithAtSymbol_ShouldInsertAtSymbolAtBeginning()
		{
			// Arrange
			var paramNameRule = new ParamNameRule("@");
			var paramName = "parameter";

			// Act
			var result = paramNameRule.GetParamName(paramName);

			// Assert
			Assert.That(result, Is.EqualTo("@parameter"));
		}

		[Test]
		public void GetParamName_WhenParamNameStartsWithAtSymbol_ShouldReturnOriginalParamName()
		{
			// Arrange
			var paramNameRule = new ParamNameRule("@");
			var paramName = "@parameter";

			// Act
			var result = paramNameRule.GetParamName(paramName);

			// Assert
			Assert.That(result, Is.EqualTo("@parameter"));
		}

		[Test]
		public void GetParamName_WhenTokenIsEmpty_ShouldReturnOriginalParamName()
		{
			// Arrange
			var paramNameRule = new ParamNameRule(string.Empty);
			var paramName = "parameter";

			// Act
			var result = paramNameRule.GetParamName(paramName);

			// Assert
			Assert.That(result, Is.EqualTo("parameter"));
		}

		[Test]
		public void GetParamName_WhenParamNameIsNull_ShouldThrowArgumentNullException()
		{
			// Arrange
			var paramNameRule = new ParamNameRule("@");

			// Act & Assert
			Assert.That(() => paramNameRule.GetParamName(null), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void GetParamName_WhenTokenIsAtSymbolAndParamNameStartsWithAtSymbol_ShouldNotAddAnotherAtSymbol()
		{
			// Arrange
			var paramNameRule = new ParamNameRule("@");
			var paramName = "@param";

			// Act
			var result = paramNameRule.GetParamName(paramName);

			// Assert
			Assert.That(result, Is.EqualTo("@param"));
		}

		[Test]
		public void GetParamName_WhenTokenIsAtSymbolAndParamNameDoesNotStartWithAtSymbol_ShouldPrependAtSymbol()
		{
			// Arrange
			var paramNameRule = new ParamNameRule("@");
			var paramName = "param";

			// Act
			var result = paramNameRule.GetParamName(paramName);

			// Assert
			Assert.That(result, Is.EqualTo("@param"));
		}
	}
}
