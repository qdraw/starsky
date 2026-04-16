using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.export.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.export.Services;

/// <summary>
///     Test preserve directory structure in zip exports
///     These tests verify that when exporting files, the directory structure is preserved
/// </summary>
[TestClass]
public class ExportServicePreserveSubfolderTest
{
	/// <summary>
	///     Test that when all files are in the root (no subfolders),
	///     only filenames are returned
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_AllFilesInRoot_ReturnsOnlyFileNames()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "20221210_105537_DSC07377.jpg"),
			Path.Combine("C:", "data", "testcontent", "20221210_105740_DSC07388.jpg"),
			Path.Combine("C:", "data", "testcontent", "20221210_105743_DSC07389.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.AreEqual("20221210_105537_DSC07377.jpg", fileNames[0]);
		Assert.AreEqual("20221210_105740_DSC07388.jpg", fileNames[1]);
		Assert.AreEqual("20221210_105743_DSC07389.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test that when files have subfolders, the directory structure is preserved
	///     Input: C:\data\testcontent\2022\12\file.jpg
	///     Expected: 2022\12\file.jpg
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_WithSubfolders_PreservesFolderStructure()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2022", "12", "20221210_105537_DSC07377.jpg"),
			Path.Combine("C:", "data", "testcontent", "2022", "12", "20221210_105740_DSC07388.jpg"),
			Path.Combine("C:", "data", "testcontent", "2022", "12", "2022_12_10 lange map naam test",
				"20221210_105728_DSC07386.jpg"),
			Path.Combine("C:", "data", "testcontent", "2022", "12", "2022_12_10 lange map naam test",
				"20221210_105743_DSC07389.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(4, fileNames);
		
		// Verify structure is preserved
		Assert.AreEqual(Path.Combine("2022", "12", "20221210_105537_DSC07377.jpg"), fileNames[0]);
		Assert.AreEqual(Path.Combine("2022", "12", "20221210_105740_DSC07388.jpg"), fileNames[1]);
		Assert.AreEqual(
			Path.Combine("2022", "12", "2022_12_10 lange map naam test", "20221210_105728_DSC07386.jpg"),
			fileNames[2]);
		Assert.AreEqual(
			Path.Combine("2022", "12", "2022_12_10 lange map naam test", "20221210_105743_DSC07389.jpg"),
			fileNames[3]);
	}

	/// <summary>
	///     Test mixed depth: some files in subfolders, some at root level
	///     Should preserve structure for all
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_MixedDepth_PreservesFolderStructure()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "file1.jpg"),
			Path.Combine("C:", "data", "testcontent", "subfolder", "file2.jpg"),
			Path.Combine("C:", "data", "testcontent", "file3.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		// When there are subfolders, ALL files should preserve their structure
		Assert.AreEqual(Path.Combine("subfolder", "file2.jpg"), fileNames[1]);
		Assert.AreEqual("file1.jpg", fileNames[0]);
		Assert.AreEqual("file3.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test single file with no subfolders
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SingleFileNoSubfolder_ReturnsSingleFileName()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "photo.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual("photo.jpg", fileNames[0]);
	}

	/// <summary>
	///     Test single file with subfolders
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SingleFileWithSubfolder_PreservesFolder()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2024", "01", "photo.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual(Path.Combine("2024", "01", "photo.jpg"), fileNames[0]);
	}

	/// <summary>
	///     Test with deeply nested directories
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_DeeplyNested_PreservesFolderStructure()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2024", "01", "15", "events", "wedding",
				"photo1.jpg"),
			Path.Combine("C:", "data", "testcontent", "2024", "01", "15", "events", "wedding",
				"photo2.jpg"),
			Path.Combine("C:", "data", "testcontent", "2024", "01", "15", "events", "birthday",
				"photo3.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.IsTrue(fileNames[0].Contains(Path.DirectorySeparatorChar),
			$"Expected path with subfolders, got: {fileNames[0]}");
		Assert.IsTrue(fileNames[1].Contains(Path.DirectorySeparatorChar),
			$"Expected path with subfolders, got: {fileNames[1]}");
		Assert.IsTrue(fileNames[2].Contains(Path.DirectorySeparatorChar),
			$"Expected path with subfolders, got: {fileNames[2]}");
		
		// Verify the relative paths start with year
		Assert.IsTrue(fileNames[0].StartsWith("2024"));
		Assert.IsTrue(fileNames[1].StartsWith("2024"));
		Assert.IsTrue(fileNames[2].StartsWith("2024"));
	}

	/// <summary>
	///     Test that relative path calculation works correctly
	///     when storage folder has or doesn't have trailing slash
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_StorageFolderVariations_HandlesCorrectly()
	{
		// Test without trailing slash
		var storageFolderNoTrail = Path.Combine("C:", "data", "testcontent");
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolderNoTrail },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2024", "file.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual(Path.Combine("2024", "file.jpg"), fileNames[0]);
	}

	/// <summary>
	///     Test empty file paths list
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_EmptyList_ReturnsEmptyList()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = "C:\\data\\testcontent\\" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>();

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.IsEmpty(fileNames);
	}

	/// <summary>
	///     Test with null storage folder - should fallback to just filename
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_NullStorageFolder_FallsBackToFileName()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = null },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2024", "file.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual("file.jpg", fileNames[0]);
	}

	/// <summary>
	///     Test files with XMP sidecars - should preserve structure for both
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_WithXmpFiles_PreservesFolderStructure()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2024", "01", "photo.dng"),
			Path.Combine("C:", "data", "testcontent", "2024", "01", "photo.xmp")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(2, fileNames);
		Assert.AreEqual(Path.Combine("2024", "01", "photo.dng"), fileNames[0]);
		Assert.AreEqual(Path.Combine("2024", "01", "photo.xmp"), fileNames[1]);
	}

	/// <summary>
	///     Test special characters in folder and file names
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SpecialCharactersInNames_PreservesFolderStructure()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2022_12_10 lange map naam test",
				"20221210_105728 (1)_DSC07386.jpg"),
			Path.Combine("C:", "data", "testcontent", "famille & vrienden",
				"photo (copy).jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(2, fileNames);
		Assert.IsTrue(fileNames[0].Contains("2022_12_10 lange map naam test"));
		Assert.IsTrue(fileNames[1].Contains("famille & vrienden"));
	}

	/// <summary>
	///     Test case-insensitive storage folder comparison
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_CaseInsensitiveStorageFolder_HandlesCorrectly()
	{
		// Arrange
		// Storage folder in lowercase
		var storageFolderLower = Path.Combine("C:", "data", "testcontent").ToLower() + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolderLower },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		// File paths with mixed case
		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "TestContent", "2024", "file.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		// Should still preserve the relative structure despite case mismatch
		Assert.AreEqual(Path.Combine("2024", "file.jpg"), fileNames[0]);
	}

	/// <summary>
	///     Test IsRunningTest scenario - simulating Windows paths with backslashes
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_WindowsPaths_PreservesFolderStructureCorrectly()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = @"C:\data\testcontent\" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		// Exact paths from user requirement
		var filePaths = new List<string>
		{
			@"C:\data\testcontent\2022\12\20221210_105537_20221210_105537_DSC07377.jpg",
			@"C:\data\testcontent\2022\12\20221210_105740_20221210_105740_DSC07388.jpg",
			@"C:\data\testcontent\2022\12\2022_12_10 lange map naam test\20221210_105728_20221210_105728_DSC07386.jpg",
			@"C:\data\testcontent\2022\12\2022_12_10 lange map naam test\20221210_105743_20221210_105743_DSC07389.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(4, fileNames);
		Assert.AreEqual(@"2022\12\20221210_105537_20221210_105537_DSC07377.jpg", fileNames[0]);
		Assert.AreEqual(@"2022\12\20221210_105740_20221210_105740_DSC07388.jpg", fileNames[1]);
		Assert.AreEqual(@"2022\12\2022_12_10 lange map naam test\20221210_105728_20221210_105728_DSC07386.jpg",
			fileNames[2]);
		Assert.AreEqual(@"2022\12\2022_12_10 lange map naam test\20221210_105743_20221210_105743_DSC07389.jpg",
			fileNames[3]);
	}

	/// <summary>
	///     Test that files without subfolders maintain flat structure
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_WindowsPathsNoSubfolders_ReturnsFlatStructure()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = @"C:\data\testcontent\" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			@"C:\data\testcontent\20221210_105743_20221210_105743_DSC07389.jpg",
			@"C:\data\testcontent\20221210_105740_20221210_105740_DSC07388.jpg"
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(2, fileNames);
		Assert.AreEqual("20221210_105743_20221210_105743_DSC07389.jpg", fileNames[0]);
		Assert.AreEqual("20221210_105740_20221210_105740_DSC07388.jpg", fileNames[1]);
	}

	/// <summary>
	///     Test with alternative directory separator (forward slash)
	///     This tests cross-platform compatibility
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_AlternativeDirectorySeparator_HandlesCorrectly()
	{
		// Arrange - on Windows, convert forward slashes to backslashes for consistency
		var storageFolderNormalized = "C:/data/testcontent/".Replace('/', Path.DirectorySeparatorChar);
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolderNormalized },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			"C:/data/testcontent/2024/01/photo.jpg".Replace('/', Path.DirectorySeparatorChar),
			"C:/data/testcontent/2024/02/photo.jpg".Replace('/', Path.DirectorySeparatorChar)
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(2, fileNames);
		// Results should contain directory structure
		Assert.IsTrue(fileNames[0].Contains(Path.DirectorySeparatorChar), 
			$"Expected path separator in {fileNames[0]}");
	}
}

