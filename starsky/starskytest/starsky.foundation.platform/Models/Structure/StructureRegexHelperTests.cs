using System;
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
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			StructureRegexHelper.StructureCheck("dion.ext");
		});
		// >= ArgumentException
	}

	[TestMethod]
	public void AppSettingsProviderTest_Null()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			StructureRegexHelper.StructureCheck(string.Empty));
	}
}
