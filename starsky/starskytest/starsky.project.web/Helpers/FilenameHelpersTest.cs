using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.project.web.Helpers;

[TestClass]
public sealed class FilenameHelpersTest
{
	[TestMethod]
	public void FilenamesHelper_ValidFileName()
	{
		var result = FilenamesHelper.IsValidFileName("test.jpg");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilenamesHelper_ValidFileName_StartWithUnderscore()
	{
		var result = FilenamesHelper.IsValidFileName("_.com");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void FilenamesHelper_NonValidFileName()
	{
		var result = FilenamesHelper.IsValidFileName(".jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFilePath()
	{
		var result = FilenamesHelper.GetFileName("sdfsdf/test.jpg");
		Assert.AreEqual("test.jpg", result);
	}


	[TestMethod]
	public void FilenamesHelper_GetParentPath()
	{
		var result = FilenamesHelper.GetParentPath("/yes.jpg");
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetParentPathSubDir()
	{
		var result = FilenamesHelper.GetParentPath("/sub/yes.jpg");
		Assert.AreEqual("/sub", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFileNameWithoutExtension()
	{
		var result = FilenamesHelper.GetFileNameWithoutExtension("/te_st/test.com");
		Assert.AreEqual("test", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFileNameWithoutExtension_mp4()
	{
		var result = FilenamesHelper.GetFileNameWithoutExtension("/te_st/test.mp4");
		Assert.AreEqual("test", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFileNameWithoutExtension_example2()
	{
		var result =
			FilenamesHelper.GetFileNameWithoutExtension(
				"http://path/Lists/Test/Attachments/1/Document Test.docx");
		Assert.AreEqual("Document Test", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFileNameWithoutExtension_example3()
	{
		var result =
			FilenamesHelper.GetFileNameWithoutExtension(
				"/0000000000aaaaa__exifreadingtest00.jpg");
		Assert.AreEqual("0000000000aaaaa__exifreadingtest00", result);
	}

	[TestMethod]
	public void FilenamesHelper_GetFileNameWithoutExtension_example4()
	{
		var result =
			FilenamesHelper.GetFileNameWithoutExtension("/0000000000aaaaa__exifreadingtest00");
		Assert.AreEqual("0000000000aaaaa__exifreadingtest00", result);
	}

	[TestMethod]
	[DataRow("/folder/test.mp4", "mp4", "test")]
	[DataRow("/test.mp4", "mp4", "test")] // lowercase
	[DataRow("/test.MP4", "mp4", "test")] // uppercase
	[DataRow("/test.jpeg", "jpeg", "test")]
	[DataRow("/test_image", "", "test_image")] // no ext
	[DataRow("/test.jpeg", "jpeg", "test")]
	[DataRow("/test.jpg.php", "php", "test.jpg")]
	[DataRow("/test.php%00.jpg", "jpg", "test.php%00")]
	[DataRow("/folder/.hiddenfile", "nfile", ".hiddenfile")] // hidden file with no extension
	[DataRow("/folder/.hiddenfile.jpg", "jpg", ".hiddenfile")] // hidden file with extension
	[DataRow("/folder/file.with.multiple.dots.ext", "ext",
		"file.with.multiple.dots")] // file with multiple dots
	[DataRow("/folder/file_with_underscore.ext", "ext",
		"file_with_underscore")] // file with underscore
	[DataRow("/folder/file-with-dash.ext", "ext", "file-with-dash")] // file with dash
	[DataRow("/folder/file with spaces.ext", "ext", "file with spaces")] // file with spaces
	[DataRow("/folder/file.ext?query=param", "ext", "file")] // file with query parameters
	[DataRow("/folder/file.ext#fragment", "ext", "file")] // file with fragment
	[DataRow("/folder/file.ext/", "", "")] // file with trailing slash
	[DataRow("/folder/file.ext.", "", "file.ext.")] // file with trailing dot
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx", "docx", "Document Test")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test", "", "Document Test")] // no ext
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.", "", "Document Test.")] // no ext
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx?query=param", "docx",
		"Document Test")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx#fragment", "docx",
		"Document Test")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx/", "",
		"")] // file with trailing slash
	public void FilenamesHelper_GetFileExtensionWithoutDot_FileName(string filePath,
		string expectedExtension, string expectedFileName)
	{
		var result = FilenamesHelper.GetFileExtensionWithoutDot(filePath);
		var fileName = FilenamesHelper.GetFileNameWithoutExtension(filePath);

		Assert.AreEqual(expectedExtension, result);
		Assert.AreEqual(expectedFileName, fileName);
	}

	[TestMethod]
	[DataRow("test.jpg", true)]
	[DataRow("_.com", true)]
	[DataRow(".jpg", false)]
	[DataRow("test", false)] // no extension
	[DataRow("test.", false)] // trailing dot
	[DataRow("test..jpg", false)] // double dot
	[DataRow("test.jpg.", false)] // trailing dot after extension
	[DataRow("test.jpg.exe", true)] // double extension
	[DataRow("test@.jpg", false)] // special character in name
	[DataRow("test jpg", false)] // space in name
	[DataRow("test.jpg ", false)] // trailing space
	[DataRow(" test.jpg", false)] // leading space
	[DataRow("test.jpg/another.jpg", false)] // slash in name
	[DataRow("test.jpg\\another.jpg", false)] // backslash in name
	[DataRow("test..", false)] // double trailing dot
	[DataRow("test..jpg", false)] // double dot before extension
	[DataRow("test.jpg..", false)] // double trailing dot after extension
	[DataRow("test.jpg..exe", false)] // double extension with dot
	[DataRow("test@jpg", false)] // special character without dot
	[DataRow("test jpg", false)] // space without dot
	[DataRow("testjpg", false)] // no dot
	[DataRow("testjpg.", false)] // trailing dot without extension
	[DataRow("testjpg..", false)] // double trailing dot without extension
	[DataRow("testjpg..exe", false)] // double extension without dot
	public void FilenamesHelper_IsValidFileName(string filename, bool expected)
	{
		var result = FilenamesHelper.IsValidFileName(filename);
		Assert.AreEqual(expected, result);
	}
}
