using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsImportTransformationModelTests
{
	[TestMethod]
	public void ToString_NoRules_ReturnsNoRulesMessage()
	{
		// Arrange
		var fakeModel = new AppSettingsImportTransformationModel();

		// Act
		var result = fakeModel.ToString();

		// Assert
		Assert.AreEqual("Rules: [No Rules]", result);
	}

	[TestMethod]
	public void ToString_WithRules_ReturnsFormattedString()
	{
		// Arrange
		var fakeModel = new AppSettingsImportTransformationModel();
		fakeModel.Rules.Add(new TransformationRule
		{
			Conditions = new TransformationConditions
			{
				Origin = "/test",
				ImageFormats =
				[
					ExtensionRolesHelper.ImageFormat.jpg,
					ExtensionRolesHelper.ImageFormat.png
				]
			}
		});

		// Act
		var result = fakeModel.ToString();

		// Assert
		Assert.AreEqual("Rules: [Condition: Origin=/test, Formats=jpg,png]", result);
	}
}
