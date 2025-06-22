using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.feature.import.Helpers;

[TestClass]
public class UpdateImportSettingsHelperTests
{
	[TestMethod]
	public void ColorClassTransformation_NullFileIndexItem_ShouldUseDefaultImageFormat()
	{
		// Arrange
		var appSettings = new AppSettings();
		var helper = new UpdateImportSettingsHelper(appSettings);

		// Act
		var result = helper.ColorClassTransformation(1, null, "test-origin");

		// Assert
		Assert.AreEqual(( ColorClassParser.Color ) 1, result);
	}

	[TestMethod]
	public void ColorClassTransformation_NullOrigin_ShouldUseDefaultSetting()
	{
		// Arrange
		var appSettings = new AppSettings();
		var helper = new UpdateImportSettingsHelper(appSettings);
		var fileIndexItem = new FileIndexItem();

		// Act
		var result = helper.ColorClassTransformation(-1, fileIndexItem,
			string.Empty);

		// Assert
		Assert.AreEqual(ColorClassParser.Color.DoNotChange, result);
	}

	[TestMethod]
	public void GetTransformationSetting_NullConfig_ShouldReturnDefaultRule()
	{
		// Act
		var result = UpdateImportSettingsHelper.GetTransformationSetting(null,
			ExtensionRolesHelper.ImageFormat.jpg, "test-origin");

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(ColorClassParser.Color.DoNotChange, result.ColorClass);
	}

	[TestMethod]
	public void GetTransformationSetting_NullOrigin_ShouldMatchImageFormat()
	{
		// Arrange
		var config = new AppSettingsImportTransformationModel();

		// Act
		var result =
			UpdateImportSettingsHelper.GetTransformationSetting(config,
				ExtensionRolesHelper.ImageFormat.jpg, string.Empty);

		// Assert
		Assert.IsNotNull(result);
	}
}
