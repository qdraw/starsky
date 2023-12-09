using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnZipFile12;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public sealed class ZipperTest
	{
		[TestMethod]
		public void NotFound()
		{
			var result =  new Zipper().ExtractZip("not-found","t");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void TestExtractZip()
		{
			// Arrange
			var zipped = CreateAnZipFile12.Bytes;

			// Act
			var result = Zipper.ExtractZip(zipped.ToArray());

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(CreateAnZipFile12.Content.Count, result.Count);

			foreach (var entry in CreateAnZipFile12.Content)
			{
				Assert.IsTrue(result.ContainsKey(entry.Key));
				var resultText = Encoding.UTF8.GetString(result[entry.Key]);
				Assert.AreEqual(entry.Value, resultText);
			}
		}
		
		[TestMethod]
		public void CreateZip_Returns_Valid_Path_When_File_Exists()
		{
			// Arrange
			List<string> filePaths = new List<string> { "C:\\temp\\file1.txt", "C:\\temp\\file2.txt" };
			List<string> fileNames = new List<string> { "file1.txt", "file2.txt" };
			const string zipOutputFilename = "CreateZip_Returns_Valid_Path_When_File_Exists";
			var tempFileFullPath = Path.Combine(new CreateAnImage().BasePath, zipOutputFilename) + ".zip";
			File.Create(tempFileFullPath).Close(); // Create a temporary zip file
			
			// Act
			var result = new Zipper().CreateZip(new CreateAnImage().BasePath, filePaths, fileNames, zipOutputFilename);

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
				string tempFileFullPath1 = Path.Combine(new CreateAnImage().BasePath, singleFile);
				File.Create(tempFileFullPath1).Close(); // Create a temporary zip file
				filePaths.Add(tempFileFullPath1);
			}
			
			const string zipOutputFilename = "CreateZip_Creates_Valid_Zip_File";
			string tempFileFullPath = Path.Combine(new CreateAnImage().BasePath, zipOutputFilename) + ".zip";

			// Act
			string result = new Zipper().CreateZip(new CreateAnImage().BasePath, filePaths, fileNames, zipOutputFilename);

			// Assert
			Assert.IsTrue(File.Exists(result));

			foreach ( var path in filePaths )
			{
				File.Delete(path);
			}

			var archive = ZipFile.OpenRead(result);
			Assert.AreEqual(filePaths.Count, archive.Entries.Count);

			for (var i = 0; i < filePaths.Count; i++)
			{
				var entry = archive.Entries[i];
				Assert.AreEqual(fileNames[i], entry.Name);
				Assert.AreEqual(0, entry.Length);
			}
			archive.Dispose();
			
			File.Delete(tempFileFullPath);
		}
	}
}
