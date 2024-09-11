using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class PathHelperTests
{
	private const string TestContentVeryLongString =
		"this-is-a-really-long-slug-that-goes-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-and-on-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-and-on-and-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-" +
		"and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-" +
		"and-and-on-and-on-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-and-on-" +
		"and-on-and-on-and-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-" +
		"and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and" +
		"-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on" +
		"-and-on-and-on-and-on-and-on-and-and-on-and-on-and-on-and-on-and-on-and-on-and-" +
		"on-and-on-and-on-and-and-on-and-and-on-and-on-and-on-and-and-on-and-and-on-and-" +
		"on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-on-and-and-on-and" +
		"-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and" +
		"-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and" +
		"-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and" +
		"-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and-and";

	[DataTestMethod] // [Theory]
	[DataRow("path/to/file.txt", "file.txt")]
	[DataRow("file.txt", "file.txt")]
	[DataRow("/file.txt", "file.txt")]
	[DataRow("/test/file.txt", "file.txt")]
	[DataRow("/test/file/", "")]
	[DataRow("/test/file", "file")] // no backslash
	public void GetFileName_ReturnsValidFileName(string input, string expectedFileName)
	{
		// Act
		var actualFileName = PathHelper.GetFileName(input);

		// Assert
		Assert.AreEqual(expectedFileName, actualFileName);
	}

	[DataTestMethod] // [Theory]
	[DataRow("path/to/file.txt", "file.txt")]
	[DataRow("/test/file/", "")]
	[DataRow("/test/file", "file")] // no backslash
	[DataRow("", "")]
	public void GetFileNameUnix_ReturnsValidFileName(string input, string expectedFileName)
	{
		// Act
		var actualFileName = PathHelper.GetFileNameUnix(input).ToString();

		// Assert
		Assert.AreEqual(expectedFileName, actualFileName);
	}

	[TestMethod]
	public void GetFileName_ReturnsFileName_WithMaliciousInput()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() =>
		{
			PathHelper.GetFileName(TestContentVeryLongString);
		});
	}

	[TestMethod]
	public void RemoveLatestBackslash_ReturnsBasePathWithoutLatestBackslash()
	{
		// Arrange
		var basePath =
			$"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory{Path.DirectorySeparatorChar}";
		var expectedPath =
			$"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory";

		// Act
		var actualPath = PathHelper.RemoveLatestBackslash(basePath);

		// Assert
		Assert.AreEqual(expectedPath, actualPath);
	}

	[TestMethod]
	public void RemoveLatestBackslash_ReturnsBasePath_WhenNoLatestBackslashExists()
	{
		// Arrange
		var basePath =
			$"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory";

		// Act
		var actualPath = PathHelper.RemoveLatestBackslash(basePath);

		// Assert
		Assert.AreEqual(basePath, actualPath);
	}

	[TestMethod]
	public void RemoveLatestBackslash_ReturnsNull_WhenBasePathIsNull()
	{
		// Act
		var actualPath = PathHelper.RemoveLatestBackslash(null!);

		// Assert
		Assert.IsNull(actualPath);
	}

	[TestMethod]
	public void RemoveLatestBackslash_ReturnsBasePath_WhenBasePathIsRoot()
	{
		// Arrange
		const string basePath = "/";

		// Act
		var actualPath = PathHelper.RemoveLatestBackslash();

		// Assert
		Assert.AreEqual(basePath, actualPath);
	}

	[TestMethod]
	public void RemoveLatestSlash_RemovesLatestSlash_WhenSlashExists()
	{
		// Arrange
		const string basePath = "/path/to/directory/";
		const string expectedPath = "/path/to/directory";

		// Act
		var actualPath = PathHelper.RemoveLatestSlash(basePath);

		// Assert
		Assert.AreEqual(expectedPath, actualPath);
	}

	[TestMethod]
	public void RemoveLatestSlash_DoesNotRemoveSlash_WhenSlashDoesNotExist()
	{
		// Arrange
		const string basePath = "/path/to/directory";

		// Act
		var actualPath = PathHelper.RemoveLatestSlash(basePath);

		// Assert
		Assert.AreEqual(basePath, actualPath);
	}

	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsNull()
	{
		// Act
		var actualPath = PathHelper.RemoveLatestSlash(null!);

		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}

	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsEmpty()
	{
		// Arrange
		var basePath = string.Empty;

		// Act
		var actualPath = PathHelper.RemoveLatestSlash(basePath);

		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}

	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsRoot()
	{
		// Arrange
		const string basePath = "/";

		// Act
		var actualPath = PathHelper.RemoveLatestSlash(basePath);

		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}

	[TestMethod]
	public void AddBackslash_AddsBackslash_WhenBackslashDoesNotExist()
	{
		// Arrange
		var thumbnailTempFolder =
			$"C:{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}" +
			$"to{Path.DirectorySeparatorChar}directory";
		var expectedPath = $"C:{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}" +
		                   $"to{Path.DirectorySeparatorChar}directory{Path.DirectorySeparatorChar}";

		// Act
		var actualPath = PathHelper.AddBackslash(thumbnailTempFolder);

		// Assert
		Assert.AreEqual(expectedPath, actualPath);
	}

	[TestMethod]
	public void AddBackslash_DoesNotAddBackslash_WhenBackslashExists()
	{
		// Arrange
		var thumbnailTempFolder =
			$"C:{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}" +
			$"to{Path.DirectorySeparatorChar}directory{Path.DirectorySeparatorChar}";

		// Act
		var actualPath = PathHelper.AddBackslash(thumbnailTempFolder);

		// Assert
		Assert.AreEqual(thumbnailTempFolder, actualPath);
	}

	[TestMethod]
	public void AddBackslash_ReturnsOriginalString_WhenInputIsNull()
	{
		// Act
		var actualPath = PathHelper.AddBackslash(null!);

		// Assert
		Assert.AreEqual(null, actualPath);
	}

	[TestMethod]
	public void AddBackslash_ReturnsOriginalString_WhenInputIsEmpty()
	{
		// Arrange
		var thumbnailTempFolder = string.Empty;

		// Act
		var actualPath = PathHelper.AddBackslash(thumbnailTempFolder);

		// Assert
		Assert.AreEqual(thumbnailTempFolder, actualPath);
	}

	[TestMethod]
	public void PrefixDbSlash_WhenCalledWithNull_ReturnsSlash()
	{
		// Act
		var result = PathHelper.PrefixDbSlash(null!);

		// Assert
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void PrefixDbSlash_WhenCalledWithEmptyString_ReturnsSlash()
	{
		// Arrange
		const string subPath = "";

		// Act
		var result = PathHelper.PrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void PrefixDbSlash_WhenCalledWithValidPath_ReturnsPathWithPrefixSlash()
	{
		// Arrange
		const string subPath = "test/subfolder/file.txt";
		const string expected = "/test/subfolder/file.txt";

		// Act
		var result = PathHelper.PrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PrefixDbSlash_WhenCalledWithAlreadyPrefixedPath_ReturnsPathWithoutChanges()
	{
		// Arrange
		const string subPath = "/test/subfolder/file.txt";
		const string expected = "/test/subfolder/file.txt";

		// Act
		var result = PathHelper.PrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestRemovePrefixDbSlash_WithLeadingSlash()
	{
		// Arrange
		const string subPath = "/path/to/file";

		// Act
		var result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("path/to/file", result);
	}

	[TestMethod]
	public void TestRemovePrefixDbSlash_WithoutLeadingSlash()
	{
		// Arrange
		const string subPath = "path/to/file";

		// Act
		var result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("path/to/file", result);
	}

	[TestMethod]
	public void TestRemovePrefixDbSlash_OnlyLeadingSlash()
	{
		// Arrange
		const string subPath = "/";

		// Act
		var result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void TestRemovePrefixDbSlash_EmptyString()
	{
		// Arrange
		const string subPath = "";

		// Act
		var result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void TestSplitInputFilePaths_WithNull_ReturnsEmptyArray()
	{
		// Act
		var result = PathHelper.SplitInputFilePaths(null!);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Length);
	}

	[TestMethod]
	public void TestSplitInputFilePaths_WithEmptyString_ReturnsEmptyArray()
	{
		// Arrange
		const string input = "";

		// Act
		var result = PathHelper.SplitInputFilePaths(input);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Length);
	}

	[TestMethod]
	public void TestSplitInputFilePaths_WithValidInput_ReturnsArrayOfStrings()
	{
		// Arrange
		const string input = "/path/to/file1;/path/to/file2;";

		// Act
		var result = PathHelper.SplitInputFilePaths(input);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Length);
		Assert.AreEqual("/path/to/file1", result[0]);
		Assert.AreEqual("/path/to/file2", result[1]);
	}
}
