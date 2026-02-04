using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;

namespace starskytest.starsky.foundation.storage.Models;

[TestClass]
public class FullFilePathExistsResultModelTests
{
	[TestMethod]
	public void DeconstructTest_Defaults()
	{
		var (ok, fullFilePath, useTempStorageForInput, fileHashWithExtension) =
			new FullFilePathExistsResultModel();

		Assert.IsFalse(ok);
		Assert.IsFalse(useTempStorageForInput);
		Assert.AreEqual(string.Empty, fullFilePath);
		Assert.AreEqual(string.Empty, fileHashWithExtension);
	}
	
	[TestMethod]
	public void DeconstructTest_Values()
	{
		var (ok, fullFilePath, useTempStorageForInput, fileHashWithExtension) =
			new FullFilePathExistsResultModel(true, "test.jpg", true, "testhash.jpg");

		Assert.IsTrue(ok);
		Assert.IsTrue(useTempStorageForInput);
		Assert.AreEqual("test.jpg", fullFilePath);
		Assert.AreEqual("testhash.jpg", fileHashWithExtension);
	}
}
