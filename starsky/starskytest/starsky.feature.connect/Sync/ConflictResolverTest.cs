using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.connect.Sync;

namespace starskytest.starsky.feature.connect.Sync;

[TestClass]
public sealed class ConflictResolverTest
{
	private string _dir = string.Empty;

	[TestInitialize]
	public void Setup()
	{
		_dir = Path.Combine(Path.GetTempPath(), "conflicttest_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_dir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_dir) )
		{
			Directory.Delete(_dir, recursive: true);
		}
	}

	[TestMethod]
	public void RenameToConflict_RenamesFile_AndReturnsNewPath()
	{
		var resolver = new ConflictResolver();
		var filePath = Path.Combine(_dir, "photo.jpg");
		File.WriteAllBytes(filePath, [1, 2, 3]);

		var result = resolver.RenameToConflict(filePath, "ABCDEFG-HIJKLMN");

		Assert.IsFalse(File.Exists(filePath), "Original file should be gone");
		Assert.IsTrue(File.Exists(result), "Conflict file should exist");
		StringAssert.Contains(result, ".sync-conflict-");
		StringAssert.Contains(result, "ABCDEFG");
		Assert.AreEqual(".jpg", Path.GetExtension(result));
	}

	[TestMethod]
	public void RenameToConflict_PreservesExtension()
	{
		var resolver = new ConflictResolver();
		var filePath = Path.Combine(_dir, "document.pdf");
		File.WriteAllText(filePath, "content");

		var result = resolver.RenameToConflict(filePath, "DEVICE1-XXXXXXXXXXX");

		Assert.AreEqual(".pdf", Path.GetExtension(result));
	}

	[TestMethod]
	public void RenameToConflict_WorksWithShortDeviceId()
	{
		var resolver = new ConflictResolver();
		var filePath = Path.Combine(_dir, "file.txt");
		File.WriteAllText(filePath, "hi");

		// Device ID with only 5 chars after removing hyphens
		var result = resolver.RenameToConflict(filePath, "AB-CD");

		Assert.IsFalse(File.Exists(filePath));
		Assert.IsTrue(File.Exists(result));
		StringAssert.Contains(result, "ABCD");
	}

	[TestMethod]
	public void RenameToConflict_IncludesTimestampInName()
	{
		var resolver = new ConflictResolver();
		var filePath = Path.Combine(_dir, "img.png");
		File.WriteAllText(filePath, "data");

		var before = DateTime.UtcNow;
		var result = resolver.RenameToConflict(filePath, "DEV12345678901234");
		var after = DateTime.UtcNow;

		var fileName = Path.GetFileName(result);
		// timestamp format: yyyyMMdd-HHmmss
		var dateStr = before.ToString("yyyyMMdd");
		StringAssert.Contains(fileName, dateStr);
	}

	[TestMethod]
	public void RenameToConflict_StemIsPreserved()
	{
		var resolver = new ConflictResolver();
		var filePath = Path.Combine(_dir, "myfile.txt");
		File.WriteAllText(filePath, "x");

		var result = resolver.RenameToConflict(filePath, "DEVICE1XXXXXXXX");

		var fileName = Path.GetFileName(result);
		StringAssert.StartsWith(fileName, "myfile.sync-conflict-");
	}
}
