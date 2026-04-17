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
			Path.Combine("C:", "data", "testcontent", "2022", "12",
				"2022_12_10 lange map naam test",
				"20221210_105728_DSC07386.jpg"),
			Path.Combine("C:", "data", "testcontent", "2022", "12",
				"2022_12_10 lange map naam test",
				"20221210_105743_DSC07389.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(4, fileNames);

		Assert.AreEqual("20221210_105537_DSC07377.jpg", fileNames[0]);
		Assert.AreEqual("20221210_105740_DSC07388.jpg", fileNames[1]);
		Assert.AreEqual(
			"2022_12_10 lange map naam test/20221210_105728_DSC07386.jpg",
			fileNames[2]);
		Assert.AreEqual(
			"2022_12_10 lange map naam test/20221210_105743_DSC07389.jpg",
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

		Assert.AreEqual("file1.jpg", fileNames[0]);
		Assert.AreEqual("subfolder/file2.jpg", fileNames[1]);
		Assert.AreEqual("file3.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test a single file with no subfolders
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SingleFileNoSubfolder_ReturnsSingleFileName()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent") + Path.DirectorySeparatorChar;
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string> { Path.Combine("C:", "data", "testcontent", "photo.jpg") };

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(1, fileNames);
		Assert.AreEqual("photo.jpg", fileNames[0]);
	}

	/// <summary>
	///     Test a single file with subfolders
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_SingleFileWithSubfolder_NoChildFolder()
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
		Assert.AreEqual("photo.jpg", fileNames[0]);
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

		Assert.AreEqual("wedding/photo1.jpg", fileNames[0]);
		Assert.AreEqual("wedding/photo2.jpg", fileNames[1]);
		Assert.AreEqual("birthday/photo3.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test that relative path calculation works correctly
	///     when a storage folder has or doesn't have a trailing slash
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
		Assert.AreEqual("file.jpg", fileNames[0]);
	}

	/// <summary>
	///     Test empty a file paths list
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_EmptyList_ReturnsEmptyList()
	{
		// Arrange
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = @"C:\data\testcontent\" },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync([], false);

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
			new AppSettings { StorageFolder = null! },
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
	public async Task FilePathToFileNameAsync_WithXmpFiles_KeepInRoot()
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

		Assert.AreEqual("photo.dng", fileNames[0]);
		Assert.AreEqual("photo.xmp", fileNames[1]);
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
		Assert.Contains("2022_12_10 lange map naam test", fileNames[0]);
		Assert.Contains("famille & vrienden", fileNames[1]);
	}


	/// <summary>
	///     Test with alternative directory separator (forward slash)
	///     This tests cross-platform compatibility
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_AlternativeDirectorySeparator_HandlesCorrectly()
	{
		// Arrange - on Windows, convert forward slashes to backslashes for consistency
		var storageFolderNormalized =
			"C:/data/testcontent/".Replace('/', Path.DirectorySeparatorChar);
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

		Assert.AreEqual("01/photo.jpg", fileNames[0]);
		Assert.AreEqual("02/photo.jpg", fileNames[1]);
	}

	/// <summary>
	///     Test case 1: When files are in a common directory with multiple children
	///     Input paths from storage: 2025/06/2025_06_18/image.jpg, 2025/06/2025_06_14/image.jpg
	///     Expected: 2025_06_18/image.jpg, 2025_06_14/image.jpg
	///     (Common ancestor "2025/06/" is stripped)
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_CommonAncestorSingleLevel_StripsSingleLevel()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent");
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2025", "06", "2025_06_18", "image.jpg"),
			Path.Combine("C:", "data", "testcontent", "2025", "06", "2025_06_14", "image.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(2, fileNames);
		Assert.AreEqual("2025_06_18/image.jpg", fileNames[0]);
		Assert.AreEqual("2025_06_14/image.jpg", fileNames[1]);
	}

	/// <summary>
	///     Test case 2: When files diverge at the month level
	///     Input paths from storage: 2025/06/2025_06_18/image.jpg, 2025/06/2025_06_14/image.jpg, 2025/07/2025_06_14/image.jpg
	///     Expected: 06/2025_06_18/image.jpg, 06/2025_06_14/image.jpg, 07/2025_06_14/image.jpg
	///     (Common ancestor "2025/" is stripped)
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_CommonAncestorTwoLevels_StripsOneLevel()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent");
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2025", "06", "2025_06_18", "image.jpg"),
			Path.Combine("C:", "data", "testcontent", "2025", "06", "2025_06_14", "image.jpg"),
			Path.Combine("C:", "data", "testcontent", "2025", "07", "2025_06_14", "image.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.AreEqual("06/2025_06_18/image.jpg", fileNames[0]);
		Assert.AreEqual("06/2025_06_14/image.jpg", fileNames[1]);
		Assert.AreEqual("07/2025_06_14/image.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test case 3: When files diverge at the year level (no common ancestor)
	///     Input paths from storage: 2026/06/2025_06_18/image.jpg, 2025/06/2025_06_14/image.jpg, 2025/07/2025_06_14/image.jpg
	///     Expected: 2026/06/2025_06_18/image.jpg, 2025/06/2025_06_14/image.jpg, 2025/07/2025_06_14/image.jpg
	///     (No common ancestor, so full paths are kept)
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_NoCommonAncestor_KeepsFullPaths()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent");
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine("C:", "data", "testcontent", "2026", "06", "2025_06_18", "image.jpg"),
			Path.Combine("C:", "data", "testcontent", "2025", "06", "2025_06_14", "image.jpg"),
			Path.Combine("C:", "data", "testcontent", "2025", "07", "2025_06_14", "image.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		Assert.AreEqual("2026/06/2025_06_18/image.jpg", fileNames[0]);
		Assert.AreEqual("2025/06/2025_06_14/image.jpg", fileNames[1]);
		Assert.AreEqual("2025/07/2025_06_14/image.jpg", fileNames[2]);
	}

	/// <summary>
	///     Test case: include a directory path (no filename) plus files inside that directory.
	///     The directory entry itself should NOT be trimmed by the common ancestor logic
	///     (it does not start with commonAncestor + "/"), while the child files should be trimmed.
	/// </summary>
	[TestMethod]
	public async Task FilePathToFileNameAsync_DirectoryAndFiles_DirectoryRemainsUnchanged()
	{
		// Arrange
		var storageFolder = Path.Combine("C:", "data", "testcontent");
		var exportService = new ExportService(new FakeIQuery(),
			new AppSettings { StorageFolder = storageFolder },
			new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailService());

		var filePaths = new List<string>
		{
			Path.Combine(storageFolder, "2025", "06", "2025_06_18"),
			Path.Combine(storageFolder, "2025", "06", "2025_06_18", "image.jpg"),
			Path.Combine(storageFolder, "2025", "06", "2025_06_18", "image2.jpg")
		};

		// Act
		var fileNames = await exportService.FilePathToFileNameAsync(filePaths, false);

		// Assert
		Assert.HasCount(3, fileNames);
		// The directory entry itself should remain unchanged (no trailing slash present)
		Assert.AreEqual("2025/06/2025_06_18", fileNames[0]);
		// Child files should be trimmed to filenames
		Assert.AreEqual("image.jpg", fileNames[1]);
		Assert.AreEqual("image2.jpg", fileNames[2]);
	}
}
