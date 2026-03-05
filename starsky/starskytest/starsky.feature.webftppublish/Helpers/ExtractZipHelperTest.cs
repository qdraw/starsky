using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Helpers;

[TestClass]
public class ExtractZipHelperTest
{
	[TestMethod]
	public void ExtractZip_Folder_ReturnsPathWithNoCleanup()
	{
		var storage = new FakeIStorage(["/test"], []);
		var helper = new ExtractZipHelper(storage, new FakeIWebLogger());

		var result = helper.ExtractZip("/test");

		Assert.IsFalse(result.IsError);
		Assert.AreEqual("/test", result.FullFileFolderPath);
		Assert.IsFalse(result.RemoveFolderAfterwards);
	}

	[TestMethod]
	public void ExtractZip_FileNotFound_ReturnsError()
	{
		var storage = new FakeIStorage();
		var helper = new ExtractZipHelper(storage, new FakeIWebLogger());

		var result = helper.ExtractZip("/nonexistent.zip");

		Assert.IsTrue(result.IsError);
		Assert.AreEqual("/nonexistent.zip", result.FullFileFolderPath);
	}

	[TestMethod]
	public void ExtractZip_CustomTempFolderPrefix()
	{
		var storage = new FakeIStorage(["/test"], []);
		var helper = new ExtractZipHelper(storage, new FakeIWebLogger());

		var result = helper.ExtractZip("/test", "custom-prefix");

		Assert.IsFalse(result.IsError);
		Assert.IsFalse(result.RemoveFolderAfterwards);
	}
}
