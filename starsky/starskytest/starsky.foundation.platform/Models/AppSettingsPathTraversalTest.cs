using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public sealed class AppSettingsPathTraversalTest
{
	[TestMethod]
	[DataRow("/../outside.jpg")]
	[DataRow("/sub/../../outside.jpg")]
	[DataRow("/../../../../etc/passwd")]
	public void DatabasePathToFilePath_DefaultStorage_DotDotStaysInsideStorageRoot(
		string databasePath)
	{
		var root = Path.Combine(Path.GetTempPath(), "starsky-traversal-test", "storage");
		var appSettings = new AppSettings { StorageFolder = root };

		string? result;
		try
		{
			result = appSettings.DatabasePathToFilePath(databasePath);
		}
		catch ( UnauthorizedAccessException )
		{
			// rejecting is the expected, safe outcome
			return;
		}

		var canonicalRoot = Path.GetFullPath(appSettings.StorageFolder);
		Assert.StartsWith(canonicalRoot, Path.GetFullPath(result),
			$"'{databasePath}' resolved to '{Path.GetFullPath(result)}', outside '{canonicalRoot}'");
	}
}
