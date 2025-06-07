using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsStructureModelTests
{
	[TestMethod]
	public void AppSettingsProviderTest_StructureCheck_MissingFirstSlash()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			AppSettingsStructureModel.StructureCheck("d/test.ext");
		});
	}

	[TestMethod]
	public void AppSettingsProviderTest_FolderWithFirstSlash()
	{
		AppSettingsStructureModel.StructureCheck("/d/dion.ext");
	}

	[TestMethod]
	public void AppSettingsProviderTest_NoFolderWithFirstSlash()
	{
		AppSettingsStructureModel.StructureCheck("/dion.ext");
	}

	[TestMethod]
	public void AppSettingsProviderTest_NoFolderMissingFirstSlash()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			AppSettingsStructureModel.StructureCheck("dion.ext");
		});
		// >= ArgumentException
	}

	[TestMethod]
	public void AppSettingsProviderTest_Null()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			AppSettingsStructureModel.StructureCheck(string.Empty));
	}
}
