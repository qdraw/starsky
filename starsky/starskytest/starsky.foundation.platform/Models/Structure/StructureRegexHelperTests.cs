using Microsoft.VisualStudio.TestTools.UnitTesting;
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
}
