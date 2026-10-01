using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage;

/// <summary>
///     Every controller (mkdir, rename, upload, sidecar upload, synchronize) ends up in
///     StorageSubPathFilesystem, so this is the shared sink for '..' in a subPath.
/// </summary>
[TestClass]
public sealed class StorageSubPathFilesystemTraversalTest
{
	[TestMethod]
	public void CreateDirectory_DotDot_DoesNotCreateFolderOutsideStorageRoot()
	{
		var testRoot = Path.Combine(Path.GetTempPath(), "starsky-sink-" + Guid.NewGuid());
		var storageRoot = Path.Combine(testRoot, "storage");
		Directory.CreateDirectory(storageRoot);
		var outside = Path.Combine(testRoot, "escaped-dir");
		try
		{
			var storage = new StorageSubPathFilesystem(
				new AppSettings { StorageFolder = storageRoot }, new FakeIWebLogger());

			try
			{
				storage.CreateDirectory("/../escaped-dir");
			}
			catch ( UnauthorizedAccessException )
			{
				// rejecting is the expected, safe outcome
			}

			Assert.IsFalse(Directory.Exists(outside),
				$"'{outside}' was created from subPath '/../escaped-dir'");
		}
		finally
		{
			Directory.Delete(testRoot, true);
		}
	}

	[TestMethod]
	public void FileMove_DotDotTarget_DoesNotMoveFileOutsideStorageRoot()
	{
		var testRoot = Path.Combine(Path.GetTempPath(), "starsky-sink-" + Guid.NewGuid());
		var storageRoot = Path.Combine(testRoot, "storage");
		Directory.CreateDirectory(storageRoot);
		File.WriteAllText(Path.Combine(storageRoot, "a.jpg"), "test");
		var outside = Path.Combine(testRoot, "moved.jpg");
		try
		{
			var storage = new StorageSubPathFilesystem(
				new AppSettings { StorageFolder = storageRoot }, new FakeIWebLogger());

			try
			{
				storage.FileMove("/a.jpg", "/../moved.jpg");
			}
			catch ( UnauthorizedAccessException )
			{
				// rejecting is the expected, safe outcome
			}

			Assert.IsFalse(File.Exists(outside), $"'{outside}' was written outside the root");
		}
		finally
		{
			Directory.Delete(testRoot, true);
		}
	}
}
