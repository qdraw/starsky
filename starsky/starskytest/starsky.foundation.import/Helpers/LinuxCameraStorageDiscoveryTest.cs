using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public sealed class LinuxCameraStorageDiscoveryTest
{
	[TestMethod]
	[DataRow("/mnt", "/mnt", 2, false)]
	[DataRow("/mnt", "/mnt/cam1", 2, true)]
	[DataRow("/mnt", "/mnt/cam1/subdir", 2, true)]
	[DataRow("/mnt", "/mnt/cam1/subdir/deep", 2, false)]
	[DataRow("/mnt", "/mnt/cam1/subdir/deep", 3, true)]
	[DataRow("/mnt/", "/mnt/cam1/", 2, true)]
	[DataRow("/", "/", 1, false)]
	[DataRow("/", "/etc", 1, true)]
	[DataRow("/media", "/media/user/device", 1, false)]
	[DataRow("/media", "/media/user/device", 2, true)]
	public void IsDirectChild_DataDriven(string basePath, string path, int maxDepth, bool expected)
	{
		var result = LinuxCameraStorageDiscovery.IsDirectChild(basePath, path, maxDepth);
		Assert.AreEqual(expected, result, $"base='{basePath}', path='{path}', maxDepth={maxDepth}");
	}
}
