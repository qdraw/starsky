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
	[DataRow("/folder/test.mp4", "mp4")]
	[DataRow("/test.mp4", "mp4")] // lowercase
	[DataRow("/test.MP4", "mp4")] // uppercase
	[DataRow("/test.jpeg", "jpeg")]
	[DataRow("/test_image", "")] // no ext
	[DataRow("/test.jpeg", "jpeg")]
	[DataRow("/test.jpg.php", "php")]
	[DataRow("/test.php%00.jpg", "jpg")]
	[DataRow("/folder/.hiddenfile", "nfile")] // hidden file with no extension
	[DataRow("/folder/.hiddenfile.jpg", "jpg")] // hidden file with extension
	[DataRow("/folder/file.with.multiple.dots.ext", "ext")] // file with multiple dots
	[DataRow("/folder/file_with_underscore.ext", "ext")] // file with underscore
	[DataRow("/folder/file-with-dash.ext", "ext")] // file with dash
	[DataRow("/folder/file with spaces.ext", "ext")] // file with spaces
	[DataRow("/folder/file.ext?query=param", "ext")] // file with query parameters
	[DataRow("/folder/file.ext#fragment", "ext")] // file with fragment
	[DataRow("/folder/file.ext/", "")] // file with trailing slash
	[DataRow("/folder/file.ext.", "")] // file with trailing dot
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx", "docx")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test", "")] // no ext
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.", "")] // no ext
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx?query=param", "docx")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx#fragment", "docx")]
	[DataRow("http://path/Lists/Test/Attachments/1/Document Test.docx/", "")] // file with trailing slash
	public void FilenamesHelper_GetFileExtensionWithoutDot(string filePath,
		string expectedExtension)
	{
		var result = FilenamesHelper.GetFileExtensionWithoutDot(filePath);
		Assert.AreEqual(expectedExtension, result);
	}
}
