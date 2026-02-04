using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsStructureModelTests
{
	[TestMethod]
	public void ToString_NoRulesNoErrors_ReturnsDefaultPatternOnly()
	{
		// Arrange
		var fakeModel = new AppSettingsStructureModel();

		// Act
		var result = fakeModel.ToString();

		// Assert
		Assert.AreEqual(
			"DefaultPattern: /yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext, " +
			"Rules: [No Rules], Errors: [No Errors]", result);
	}

	[TestMethod]
	public void ToString_WithRulesAndErrors_ReturnsFormattedString()
	{
		// Arrange
		var fakeModel = new AppSettingsStructureModel();
		fakeModel.Rules.Add(new StructureRule { Pattern = "/{filenamebase}.ext" });
		fakeModel.Errors.Add("Error 1");
		fakeModel.Errors.Add("Error 2");

		// Act
		var result = fakeModel.ToString();

		// Assert
		Assert.AreEqual(
			"DefaultPattern: /yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext, " +
			"Rules: [/{filenamebase}.ext], Errors: [Error 1; Error 2]", result);
	}
}
