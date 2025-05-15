using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;

namespace starskytest.starsky.foundation.storage.Models;

[TestClass]
public class FullFilePathExistsResultModelTests
{
	[TestMethod]
	public void DeconstructTest()
	{
		var (ok, fullFilePath, useTempStorageForInput, fileHashWithExtension) = new FullFilePathExistsResultModel();
		
		Assert.IsFalse(ok);
		Assert.IsFalse(useTempStorageForInput);
		Assert.AreEqual(string.Empty,  fullFilePath);
		Assert.AreEqual(string.Empty,  fileHashWithExtension);
	}
}
