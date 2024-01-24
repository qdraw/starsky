using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class PathHelperTests
{
	[TestMethod]
	public void GetFileName_ReturnsValidFileName()
	{
		// Arrange
		const string filePath = "path/to/file.txt";
		const string expectedFileName = "file.txt";
        
		// Act
		var actualFileName = PathHelper.GetFileName(filePath);
        
		// Assert
		Assert.AreEqual(expectedFileName, actualFileName);
	}
    
	[TestMethod]
	[ExpectedException(typeof(RegexMatchTimeoutException))]
	public async Task GetFileName_ReturnsFileName_WithMaliciousInput_UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("Test is Linux & Mac OS Only");
			return;
		}
		
		// Act and Assert
		var test = await 
			StreamToStringHelper.StreamToStringAsync(
				new MemoryStream(CreateAnImage.Bytes.ToArray()));
		var test2 = await
			StreamToStringHelper.StreamToStringAsync(
				new MemoryStream(CreateAnImageA6600.Bytes.ToArray()));
		
		PathHelper.GetFileName(test + test2 + test + test,1);
	}
	
	[TestMethod]
	public void RemoveLatestBackslash_ReturnsBasePathWithoutLatestBackslash()
	{
		// Arrange
		var basePath = $"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory{Path.DirectorySeparatorChar}";
		var expectedPath = $"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory";
        
		// Act
		var actualPath = PathHelper.RemoveLatestBackslash(basePath);
        
		// Assert
		Assert.AreEqual(expectedPath, actualPath);
	}
    
	[TestMethod]
	public void RemoveLatestBackslash_ReturnsBasePath_WhenNoLatestBackslashExists()
	{
		// Arrange
		var basePath = $"{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory";
        
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
		string basePath = "/path/to/directory/";
		string expectedPath = "/path/to/directory";
        
		// Act
		string actualPath = PathHelper.RemoveLatestSlash(basePath);
        
		// Assert
		Assert.AreEqual(expectedPath, actualPath);
	}
    
	[TestMethod]
	public void RemoveLatestSlash_DoesNotRemoveSlash_WhenSlashDoesNotExist()
	{
		// Arrange
		string basePath = "/path/to/directory";
        
		// Act
		string actualPath = PathHelper.RemoveLatestSlash(basePath);
        
		// Assert
		Assert.AreEqual(basePath, actualPath);
	}
    
	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsNull()
	{
		// Act
		string actualPath = PathHelper.RemoveLatestSlash(null!);
        
		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}
    
	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsEmpty()
	{
		// Arrange
		string basePath = string.Empty;
        
		// Act
		string actualPath = PathHelper.RemoveLatestSlash(basePath);
        
		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}
    
	[TestMethod]
	public void RemoveLatestSlash_ReturnsEmptyString_WhenBasePathIsRoot()
	{
		// Arrange
		string basePath = "/";
        
		// Act
		string actualPath = PathHelper.RemoveLatestSlash(basePath);
        
		// Assert
		Assert.AreEqual(string.Empty, actualPath);
	}
	
	[TestMethod]
	public void AddBackslash_AddsBackslash_WhenBackslashDoesNotExist()
	{
		// Arrange
		var thumbnailTempFolder = $"C:{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}" +
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
		var thumbnailTempFolder = $"C:{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}" +
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
		string result = PathHelper.PrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual(expected, result);
	}
	
	[TestMethod]
	public void TestRemovePrefixDbSlash_WithLeadingSlash()
	{
		// Arrange
		string subPath = "/path/to/file";

		// Act
		string result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("path/to/file", result);
	}
    
	[TestMethod]
	public void TestRemovePrefixDbSlash_WithoutLeadingSlash()
	{
		// Arrange
		string subPath = "path/to/file";

		// Act
		string result = PathHelper.RemovePrefixDbSlash(subPath);

		// Assert
		Assert.AreEqual("path/to/file", result);
	}
    
	[TestMethod]
	public void TestRemovePrefixDbSlash_OnlyLeadingSlash()
	{
		// Arrange
		const string subPath = "/";

		// Act
		string result = PathHelper.RemovePrefixDbSlash(subPath);

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
