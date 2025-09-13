using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Models.Structure;

namespace starskytest.starsky.foundation.platform.Models.Structure;

[TestClass]
public class AppSettingsStructureModelTests
{
	[TestMethod]
	public void AppSettingsProviderTest_StructureCheck_MissingFirstSlash()
	{
		var result = StructureRegexHelper.StructureCheck("d/test.ext");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void AppSettingsProviderTest_FolderWithFirstSlash()
	{
		StructureRegexHelper.StructureCheck("/d/dion.ext");
	}

	[TestMethod]
	public void AppSettingsProviderTest_NoFolderWithFirstSlash()
	{
		StructureRegexHelper.StructureCheck("/dion.ext");
	}

	[TestMethod]
	public void AppSettingsProviderTest_NoFolderMissingFirstSlash()
	{
		var result = StructureRegexHelper.StructureCheck("dion.ext");
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public void AppSettingsProviderTest_Null(string? value)
	{
		var result = StructureRegexHelper.StructureCheck(value);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void InvalidPattern_ShouldAddErrorToErrorsList()
	{
		// Arrange
		const string invalidPattern = "invalid_structure_pattern";
		var fakeRule = new StructureRule
		{
			// Act
			Pattern = invalidPattern
		};

		// Assert
		Assert.HasCount(1, fakeRule.Errors);
		Assert.AreEqual($"Structure '{invalidPattern}' is not valid", fakeRule.Errors[0]);
	}

	[TestMethod]
	public void Clone_ShouldReturnNewInstance()
	{
		// Arrange
		var model = new AppSettingsStructureModel();

		// Act
		var result = model.Clone();

		// Assert
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(AppSettingsStructureModel));
	}
}
