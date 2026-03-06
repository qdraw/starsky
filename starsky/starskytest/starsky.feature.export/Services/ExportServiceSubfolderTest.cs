using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.export.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.export.Services;

[TestClass]
public class ExportServiceSubfolderTest
{
	/// <summary>
	///     Test that when there are no subfolders, all files are placed in the root of the zip
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_NoSubfolders_ReturnsOnlyFileNames()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "/mnt/storage/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			"/mnt/storage/file1.jpg",
			"/mnt/storage/file2.jpg",
			"/mnt/storage/file3.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.AreEqual("file1.jpg", fileNames[0]);
		Assert.AreEqual("file2.jpg", fileNames[1]);
		Assert.AreEqual("file3.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test that when there are subfolders, the folder structure is preserved
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_WithSubfolders_PreservesFolderStructure()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "/mnt/storage/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			"/mnt/storage/2024/01/file1.jpg",
			"/mnt/storage/2024/02/file2.jpg",
			"/mnt/storage/2024/01/file3.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.Contains("/", fileNames[0]);
		Assert.Contains("/", fileNames[1]);
		Assert.Contains("/", fileNames[2]);
		Assert.Contains("2024", fileNames[0]);
		Assert.Contains("2024", fileNames[1]);
	}

	/// <summary>
	///     Test mixed depth: some files in subfolders, some at root level
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_MixedDepth_PreservesFolderStructure()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "/mnt/storage/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			"/mnt/storage/file1.jpg",
			"/mnt/storage/subfolder/file2.jpg",
			"/mnt/storage/file3.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		// When there are subfolders, all files should preserve their structure
		Assert.Contains("/", fileNames[1]);
	}

	/// <summary>
	///     Test that thumbnails preserve folder structure
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_ThumbnailsWithSubfolders_PreservesFolderStructure()
	{
		// Arrange
		var fakeQuery = new FakeIQuery();
		var exportService = new ExportService(fakeQuery,
			new AppSettings { StorageFolder = "/mnt/storage/", ThumbnailTempFolder = "/tmp/thumbs/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		// Thumbnail file paths (hash-based names)
		var filePaths = new List<string>
		{
			"/tmp/thumbs/hash1_large.jpg",
			"/tmp/thumbs/hash2_large.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, true);

		// Assert
		Assert.HasCount(2, fileNames);
	}

	/// <summary>
	///     Test single file (no subfolders)
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SingleFile_ReturnsSingleFileName()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "/mnt/storage/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string> { "/mnt/storage/photo.jpg" };

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual("photo.jpg", fileNames[0]);
	}

	/// <summary>
	///     Test with deeply nested directories
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_DeeplyNested_PreservesFolderStructure()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "/mnt/storage/" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			"/mnt/storage/2024/01/15/events/wedding/photo1.jpg",
			"/mnt/storage/2024/01/15/events/wedding/photo2.jpg",
			"/mnt/storage/2024/01/15/events/birthday/photo3.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		// All should contain multiple slashes for nested structure
		foreach ( var fileName in fileNames )
		{
			Assert.Contains("/", fileName, $"Expected path with subfolders, got: {fileName}");
		}
	}
}



