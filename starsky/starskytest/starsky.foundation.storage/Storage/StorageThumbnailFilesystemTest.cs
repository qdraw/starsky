using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public class StorageThumbnailFilesystemTest
	{
		private readonly StorageThumbnailFilesystem _thumbnailStorage;
		private readonly string _fileNameWithoutExtension;

		public StorageThumbnailFilesystemTest()
		{
			var createNewImage = new CreateAnImage();
			var appSettings = new AppSettings {ThumbnailTempFolder = createNewImage.BasePath};
			_thumbnailStorage = new StorageThumbnailFilesystem(appSettings);
			_fileNameWithoutExtension = FilenamesHelper.GetFileNameWithoutExtension(createNewImage.FileName);

		}
		
		[TestMethod]
		public void FileMove()
		{
			var createNewImage = new CreateAnImage();
			
			_thumbnailStorage.FileMove(_fileNameWithoutExtension, "StorageThumbnailFilesystemTest_FileMove");

			var path = Path.Combine(createNewImage.BasePath, _fileNameWithoutExtension + ".jpg");
			Assert.IsFalse(File.Exists(path));
			var path2 = Path.Combine(createNewImage.BasePath, "StorageThumbnailFilesystemTest_FileMove.jpg");
			Assert.IsTrue(File.Exists(path2));
			
			File.Delete(Path.Combine(createNewImage.BasePath, "StorageThumbnailFilesystemTest_FileMove.jpg"));
			new CreateAnImage();
		}

		[TestMethod]
		public void FileDelete_NotExist()
		{
			Assert.IsFalse(_thumbnailStorage.FileDelete("NotFound"));
		}
		
		[TestMethod]
		public void ReadStream()
		{
			new CreateAnImage();

			var stream =_thumbnailStorage.ReadStream(_fileNameWithoutExtension);
			Assert.AreEqual(CreateAnImage.Bytes.Length,stream.Length);
			
			stream.Dispose();
		}
		
		[TestMethod]
		public void ReadStream_MaxLength()
		{
			new CreateAnImage();

			var stream =_thumbnailStorage.ReadStream(_fileNameWithoutExtension, 100);
			Assert.AreEqual(100,stream.Length);
			
			stream.Dispose();
		}
		
		[TestMethod]
		public void WriteStream()
		{
			var createNewImage = new CreateAnImage();

			_thumbnailStorage.WriteStream(new MemoryStream(CreateAnImage.Bytes),
				"StorageThumbnailFilesystemTest_WriteStream");

			var readStream =_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStream");
			Assert.AreEqual(CreateAnImage.Bytes.Length,readStream.Length);
			readStream.Dispose();

			File.Delete(Path.Combine(createNewImage.BasePath, "StorageThumbnailFilesystemTest_FileMove.jpg"));
		}
		
		[TestMethod]
		public async Task WriteStreamAsync()
		{
			var createNewImage = new CreateAnImage();

			await _thumbnailStorage.WriteStreamAsync(new MemoryStream(CreateAnImage.Bytes),
				"StorageThumbnailFilesystemTest_WriteStreamAsync");

			var readStream =_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStreamAsync");
			Assert.AreEqual(CreateAnImage.Bytes.Length,readStream.Length);
			await readStream.DisposeAsync();

			File.Delete(Path.Combine(createNewImage.BasePath, "StorageThumbnailFilesystemTest_WriteStreamAsync.jpg"));
		}
	}
}
