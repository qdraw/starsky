using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnZipFile12;
using starskytest.FakeCreateAn.CreateAnZipFileChildFolders;
using starskytest.FakeCreateAn.CreateAnZipFileMacOs;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.ArchiveFormats;

[TestClass]
public sealed class ZipperTest
{
	private static readonly byte[] ValidZipSignatureButInvalidFile = [0x50, 0x4B, 0x03, 0x04, 0x00];

	private static readonly byte[] InvalidZip = [0x50, 0x4B, 0x03, 0x04];

	private static readonly byte[] TooShortZip = "PK"u8.ToArray();
	private static readonly byte[] EmptyZip = [];

	[TestMethod]
	public void NotFound()
	{
		var result = new Zipper(new FakeIWebLogger()).ExtractZip("not-found", "t");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TestExtractZip()
	{
		// Arrange
		var zipped = CreateAnZipFile12.Bytes;

		// Act
		var result = new Zipper(new FakeIWebLogger()).ExtractZip([.. zipped]);

		// Assert
		Assert.IsNotNull(result);
		Assert.HasCount(CreateAnZipFile12.Content.Count, result);

		foreach ( var entry in CreateAnZipFile12.Content )
		{
			Assert.IsTrue(result.ContainsKey(entry.Key));
			var resultText = Encoding.UTF8.GetString(result[entry.Key]);
			Assert.AreEqual(entry.Value, resultText);
		}
	}

	[TestMethod]
	public void TestExtractZipMacOsHiddenFiles()
	{
		// Arrange
		var zipped = new CreateAnZipFileMacOs().FilePath;
		var testOutputFolder =
			Path.Combine(new CreateAnImage().BasePath, "test-folder-zip-folders-mac-os");

		var hostService = new StorageHostFullPathFilesystem(new FakeIWebLogger());

		hostService.FolderDelete(testOutputFolder);
		hostService.CreateDirectory(testOutputFolder);

		Console.WriteLine("Zipped file:" + zipped + " ~ " + File.Exists(zipped));
		Assert.IsTrue(File.Exists(zipped));

		// Act
		var result = new Zipper(new FakeIWebLogger()).ExtractZip(zipped,
			testOutputFolder);

		// Assert
		Assert.IsTrue(result);

		var outputFile = testOutputFolder + Path.DirectorySeparatorChar +
		                 CreateAnZipFileMacOs.Content[0];

		Console.WriteLine("Output file:");
		Console.WriteLine(outputFile);

		Console.WriteLine("List content:");
		Console.WriteLine(hostService.GetAllFilesInDirectory(testOutputFolder).FirstOrDefault());
		Assert.AreEqual(1, hostService.GetAllFilesInDirectory(testOutputFolder).Count());

		Assert.IsTrue(Path.Exists(outputFile));

		hostService.FolderDelete(testOutputFolder);
	}

	[TestMethod]
	public void TestExtractZipFolders()
	{
		// Arrange
		var zipped = CreateAnZipFileChildFolders.Bytes;
		var testOutputFolder =
			Path.Combine(new CreateAnImage().BasePath, "test-folder-zip-folders");
		var inputZipPath = testOutputFolder + ".zip";
		var hostService = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		hostService.WriteStream(new MemoryStream([..zipped]), inputZipPath);
		hostService.CreateDirectory(testOutputFolder);

		// Act
		var result = new Zipper(new FakeIWebLogger()).ExtractZip(inputZipPath, testOutputFolder);

		// Assert
		Assert.IsTrue(result);

		foreach ( var contentItem in CreateAnZipFileChildFolders.Content )
		{
			var path = Path.Combine(testOutputFolder,
				contentItem.Key.Replace("/", Path.DirectorySeparatorChar.ToString()));

			if ( contentItem.Value )
			{
				Assert.IsTrue(hostService.ExistFolder(path));
				continue;
			}

			Assert.IsTrue(hostService.ExistFile(path));
		}

		hostService.FolderDelete(testOutputFolder);
		hostService.FileDelete(inputZipPath);
	}

	[TestMethod]
	public void CreateZip_Returns_Valid_Path_When_File_Exists()
	{
		// Arrange
		var filePaths = new List<string> { "C:\\temp\\file1.txt", "C:\\temp\\file2.txt" };
		var fileNames = new List<string> { "file1.txt", "file2.txt" };
		const string zipOutputFilename = "CreateZip_Returns_Valid_Path_When_File_Exists";
		var tempFileFullPath =
			Path.Combine(new CreateAnImage().BasePath, zipOutputFilename) + ".zip";
		File.Create(tempFileFullPath).Close(); // Create a temporary zip file

		// Act
		var result = new Zipper(new FakeIWebLogger()).CreateZip(new CreateAnImage().BasePath,
			filePaths, fileNames, zipOutputFilename);

		// Assert
		File.Delete(tempFileFullPath);
		Assert.AreEqual(tempFileFullPath, result);
	}

	[TestMethod]
	public void CreateZip_Creates_Valid_Zip_File()
	{
		// Arrange
		var fileNames = new List<string>
		{
			"CreateZip_Creates_Valid_Zip_File___file1.txt",
			"CreateZip_Creates_Valid_Zip_File___file2.txt"
		};
		var filePaths = new List<string>();
		foreach ( var singleFile in fileNames )
		{
			var tempFileFullPath1 = Path.Combine(new CreateAnImage().BasePath, singleFile);
			File.Create(tempFileFullPath1).Close(); // Create a temporary zip file
			filePaths.Add(tempFileFullPath1);
		}

		const string zipOutputFilename = "CreateZip_Creates_Valid_Zip_File";
		var tempFileFullPath =
			Path.Combine(new CreateAnImage().BasePath, zipOutputFilename) + ".zip";

		// Act
		var result = new Zipper(new FakeIWebLogger()).CreateZip(new CreateAnImage().BasePath,
			filePaths, fileNames, zipOutputFilename);

		// Assert
		Assert.IsTrue(File.Exists(result));

		foreach ( var path in filePaths )
		{
			File.Delete(path);
		}

		var archive = ZipFile.OpenRead(result);
		Assert.HasCount(filePaths.Count, archive.Entries);

		for ( var i = 0; i < filePaths.Count; i++ )
		{
			var entry = archive.Entries[i];
			Assert.AreEqual(fileNames[i], entry.Name);
			Assert.AreEqual(0, entry.Length);
		}

		archive.Dispose();

		File.Delete(tempFileFullPath);
	}

	[TestMethod]
	public async Task ExtractZip_ShouldReturnFalse_WhenIOExceptionOccurs()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);

		var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
		var extractPath = Path.Combine(Path.GetTempPath(), "ExtractTest2");

		if ( File.Exists(zipFilePath) )
		{
			File.Delete(zipFilePath);
		}

		Directory.CreateDirectory(extractPath);

		using ( var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create) )
		{
			var entry = zip.CreateEntry("test.txt");
			await using var writer = new StreamWriter(entry.Open());
			await writer.WriteAsync("Test content");
		}

		var lockedFilePath = Path.Combine(extractPath, "test.txt");
		var lockedFile = new FileStream(lockedFilePath, FileMode.Create,
			FileAccess.ReadWrite, FileShare.None);

		// Act
		var result = zipper.ExtractZip(zipFilePath, extractPath);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(
			logger.TrackedExceptions[^1].Item2?.Contains(
				"[Zipper] IOException"));

		// need to dispose of the locked file
		await lockedFile.DisposeAsync();

		// Cleanup
		File.Delete(zipFilePath);
		Directory.Delete(extractPath, true);
	}

	public static IEnumerable<object[]> IsValidZipFileTestData()
	{
		yield return [ValidZipSignatureButInvalidFile, true];
		yield return [InvalidZip, false];
		yield return [TooShortZip, false];
		yield return [EmptyZip, false];
	}

	[TestMethod]
	[DynamicData(nameof(IsValidZipFileTestData))]
	public void IsValidZipFile_filePath_Theory(byte[] fileContent, bool expected)
	{
		var tempFile = Path.GetTempFileName();
		File.WriteAllBytes(tempFile, fileContent);
		var result = Zipper.IsValidZipFile(tempFile);
		File.Delete(tempFile);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DynamicData(nameof(IsValidZipFileTestData))]
	public void IsValidZipFile_byte_Theory(byte[] fileContent, bool expected)
	{
		var result = Zipper.IsValidZipFile(fileContent);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void IsValidZipFile_FileDoesNotExist_ReturnsFalse()
	{
		var result = Zipper.IsValidZipFile("nonexistent.zip");
		Assert.IsFalse(result);
	}

	public static IEnumerable<object[]> ExtractZipTestData()
	{
		yield return [ValidZipSignatureButInvalidFile, false];
		yield return [InvalidZip, false];
		yield return [TooShortZip, false];
		yield return [EmptyZip, false];
	}

	[TestMethod]
	[DynamicData(nameof(ExtractZipTestData))]
	public void ExtractZip_Bytes_Theory(byte[] fileContent, bool expected)
	{
		var result = new Zipper(new FakeIWebLogger()).ExtractZip(fileContent);

		var isValid = result.Count > 0;
		Assert.AreEqual(expected, isValid);
	}

	[TestMethod]
	[DynamicData(nameof(ExtractZipTestData))]
	public void ExtractZip_FullFilePath_Theory(byte[] fileContent, bool expected)
	{
		var inputFileFullPath = Path.GetTempFileName();
		File.WriteAllBytes(inputFileFullPath, fileContent);

		var outputFileFullPath = Path.GetTempFileName();

		var result =
			new Zipper(new FakeIWebLogger()).ExtractZip(inputFileFullPath, outputFileFullPath);

		File.Delete(inputFileFullPath);
		File.Delete(outputFileFullPath);

		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void ExtractZipEntry_Found_ReturnsBytes()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);
		var tempZip = Path.Combine(Path.GetTempPath(), "zip-entry-test.zip");

		if ( File.Exists(tempZip) )
		{
			File.Delete(tempZip);
		}

		using ( var zip = ZipFile.Open(tempZip, ZipArchiveMode.Create) )
		{
			var entry = zip.CreateEntry("folder/test.txt");
			using var writer = new StreamWriter(entry.Open());
			writer.Write("hello");
		}

		var result = zipper.ExtractZipEntry(tempZip, "folder/test.txt");

		File.Delete(tempZip);
		Assert.IsNotNull(result);
		Assert.AreEqual("hello", Encoding.UTF8.GetString(result));
	}

	[TestMethod]
	public void ExtractZipEntry_NotFound_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);
		var tempZip = Path.Combine(Path.GetTempPath(), "zip-entry-missing.zip");

		if ( File.Exists(tempZip) )
		{
			File.Delete(tempZip);
		}

		using ( var zip = ZipFile.Open(tempZip, ZipArchiveMode.Create) )
		{
			zip.CreateEntry("exists.txt");
		}

		var result = zipper.ExtractZipEntry(tempZip, "missing.txt");

		File.Delete(tempZip);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractZipEntry_ZipNotFound_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);

		var result = zipper.ExtractZipEntry("/not-found.zip", "missing.txt");
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractZipEntry_ZipNotFoundAndEntryNoContent_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);

		var result = zipper.ExtractZipEntry("/not-found.zip", string.Empty);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractZipEntry_InvalidZip_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);
		var tempZip = Path.GetTempFileName();

		File.WriteAllBytes(tempZip, InvalidZip);

		var result = zipper.ExtractZipEntry(tempZip, "test.txt");

		File.Delete(tempZip);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ExtractZipEntry_InvalidZip2_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var zipper = new Zipper(logger);
		var tempZip = Path.GetTempFileName();

		File.WriteAllBytes(tempZip, ValidZipSignatureButInvalidFile);

		var result = zipper.ExtractZipEntry(tempZip, "test.txt");

		File.Delete(tempZip);
		Assert.IsNull(result);
	}
}
