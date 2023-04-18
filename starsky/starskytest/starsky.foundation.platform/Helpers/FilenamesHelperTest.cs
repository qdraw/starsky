using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class FilenamesHelperTest
{
	[TestMethod]
	public void IsValidFileName_ValidFilename_ReturnsTrue()
	{
		// Arrange
		const string filename = "example.txt";

		// Act
		var isValid = FilenamesHelper.IsValidFileName(filename);

		// Assert
		Assert.IsTrue(isValid);
	}

	[TestMethod]
	public void IsValidFileName_InvalidFilename_ReturnsFalse()
	{
		// Arrange
		const string filename = "example|invalid.txt";

		// Act
		var isValid = FilenamesHelper.IsValidFileName(filename);

		// Assert
		Assert.IsFalse(isValid);
	}
	
	[TestMethod]
	public void GetFileName_ShouldReturnCorrectFileName()
	{
		// Arrange
		const string filePath1 = "/path/to/file.txt";
		const string filePath2 = "/another/path/to/another/file.pdf";
		const string filePath3 = "file.csv";
        
		// Act
		var fileName1 = FilenamesHelper.GetFileName(filePath1);
		var fileName2 = FilenamesHelper.GetFileName(filePath2);
		var fileName3 = FilenamesHelper.GetFileName(filePath3);
        
		// Assert
		Assert.AreEqual("file.txt", fileName1);
		Assert.AreEqual("file.pdf", fileName2);
		Assert.AreEqual("file.csv", fileName3);
	}

	[TestMethod]
	public void GetFileName_ShouldReturnCorrectFileName_RuntimeOverwrite()
	{
		// Arrange
		const string filePath1 = "/path/to/file.txt";
		const string filePath2 = "/another/path/to/another/file.pdf";
		const string filePath3 = "file.csv";
        
		// Act
		var fileName1 = FilenamesHelper.GetFileName(filePath1, _ => true);
		var fileName2 = FilenamesHelper.GetFileName(filePath2, _ => true);
		var fileName3 = FilenamesHelper.GetFileName(filePath3, _ => true);
        
		// Assert
		Assert.AreEqual("file.txt", fileName1);
		Assert.AreEqual("file.pdf", fileName2);
		Assert.AreEqual("file.csv", fileName3);
	}
	
	[TestMethod]
	public void GetFileName_IgnoreEscapedValues()
	{
		// Arrange
		const string filePath1 = "/path/to/file\\d.txt";

		// Act
		var fileName1 = FilenamesHelper.GetFileName(filePath1);
		Assert.AreEqual("file\\d.txt", fileName1);
	}

	[TestMethod]
	public void GetFileName_IgnoreMultipleEscapedValues()
	{
		// Arrange
		const string filePath1 = "/path/to/file\\d\\e.txt";

		// Act
		var fileName1 = FilenamesHelper.GetFileName(filePath1);
		Assert.AreEqual("file\\d\\e.txt", fileName1);
	}

	[TestMethod]
	public void GetFileNameWithoutExtension_ValidFilePath_ReturnsFileNameWithoutExtension()
	{
		// Arrange
		string filePath = "/MyFolder/MyFile.txt";
		string expected = "MyFile";

		// Act
		string actual = FilenamesHelper.GetFileNameWithoutExtension(filePath);

		// Assert
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void GetFileNameWithoutExtension_NoExtension_ReturnsFileName()
	{
		// Arrange
		string filePath = "/MyFolder/MyFile";
		string expected = "MyFile";

		// Act
		string actual = FilenamesHelper.GetFileNameWithoutExtension(filePath);

		// Assert
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void GetFileNameWithoutExtension_MultipleDotsInName_ReturnsFileNameWithoutExtension()
	{
		// Arrange
		string filePath = "/MyFolder/My.File.Name.txt";
		string expected = "My.File.Name";

		// Act
		string actual = FilenamesHelper.GetFileNameWithoutExtension(filePath);

		// Assert
		Assert.AreEqual(expected, actual);
	}
	
	[TestMethod]
	public void GetFileExtensionWithoutDot_Should_Return_Empty_String_For_File_Without_Extension()
	{
		// Arrange
		var filename = "testfile";

		// Act
		var extension = FilenamesHelper.GetFileExtensionWithoutDot(filename);

		// Assert
		Assert.AreEqual(string.Empty, extension);
	}

	[TestMethod]
	public void GetFileExtensionWithoutDot_Should_Return_Lowercase_Extension_Without_Dot()
	{
		// Arrange
		var filename = "testfile.TXT";

		// Act
		var extension = FilenamesHelper.GetFileExtensionWithoutDot(filename);

		// Assert
		Assert.AreEqual("txt", extension);
	}

	[TestMethod]
	public void GetFileExtensionWithoutDot_Should_Return_Extension_With_Up_To_4_Alphanumeric_Characters()
	{
		// Arrange
		var filename = "testfile.some-extension01234.txt";

		// Act
		var extension = FilenamesHelper.GetFileExtensionWithoutDot(filename);

		// Assert
		Assert.AreEqual("txt", extension);
	}

	[TestMethod]
	public void GetFileExtensionWithoutDot_Should_Not_Return_Extension_With_5_Characters()
	{
		// Arrange
		var filename = "testfile.some-extension012345.txt";

		// Act
		var extension = FilenamesHelper.GetFileExtensionWithoutDot(filename);

		// Assert
		Assert.AreEqual("txt", extension);
	}

	[TestMethod]
	public void GetFileExtensionWithoutDot_Should_Not_Return_Extension_With_Invalid_Characters()
	{
		// Arrange
		const string filename = "testfile.some_extension#.txt";

		// Act
		var extension = FilenamesHelper.GetFileExtensionWithoutDot(filename);

		// Assert
		Assert.AreEqual("txt", extension);
	}
	
	[TestMethod]
	public void GetParentPath_ReturnsCorrectPath_WhenFilePathIsValid()
	{
		// Arrange
		const string filePath = "/folder1/folder2/file.txt";

		// Act
		var result = FilenamesHelper.GetParentPath(filePath);

		// Assert
		Assert.AreEqual("/folder1/folder2", result);
	}

	[TestMethod]
	public void GetParentPath_ReturnsSlash_WhenFilePathIsRoot()
	{
		// Arrange
		const string filePath = "/";

		// Act
		var result = FilenamesHelper.GetParentPath(filePath);

		// Assert
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void GetParentPath_ReturnsSlash_WhenFilePathIsNullOrEmpty()
	{
		// Arrange
		string filePath2 = "";

		// Act
		string result1 = FilenamesHelper.GetParentPath(null);
		string result2 = FilenamesHelper.GetParentPath(filePath2);

		// Assert
		Assert.AreEqual("/", result1);
		Assert.AreEqual("/", result2);
	}
}
