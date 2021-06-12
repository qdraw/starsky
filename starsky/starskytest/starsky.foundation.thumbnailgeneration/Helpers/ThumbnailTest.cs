using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Helpers
{
	[TestClass]
	public class ThumbnailTest
	{
		private readonly FakeIStorage _iStorage;
		private readonly string _fakeIStorageImageSubPath;

		public ThumbnailTest()
		{
			_fakeIStorageImageSubPath = "/test.jpg";
			
			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task CreateThumbTest_FileHash_FileHashNull()
		{
			await new Thumbnail(_iStorage, _iStorage, new FakeIWebLogger()).CreateThumb(
				"/notfound.jpg", null);
			// expect ArgumentNullException
		}

		[TestMethod]
		public async Task CreateThumbTest_FileHash_ImageSubPathNotFound()
		{
			var isCreated = await new Thumbnail(_iStorage, _iStorage, new FakeIWebLogger()).CreateThumb(
				"/notfound.jpg", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_WrongImageType()
		{
			var isCreated =  await new Thumbnail(_iStorage, _iStorage, new FakeIWebLogger()).CreateThumb(
				"/notfound.dng", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});

			var isCreated = await new Thumbnail(storage, storage, new FakeIWebLogger()).CreateThumb(
				_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}

		[TestMethod]
		public async Task CreateThumbTest_1arg_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger()).CreateThumb(
				_fakeIStorageImageSubPath);
			Assert.AreEqual(true,isCreated);
		}

		[TestMethod]
		public async Task ResizeThumbnailToStream__HostDependency__JPEG_Test()
		{
			var newImage = new CreateAnImage();
			var iStorage = new StorageHostFullPathFilesystem();

			// string subPath, int width, string outputHash = null,bool removeExif = false,ExtensionRolesHelper.ImageFormat
			// imageFormat = ExtensionRolesHelper.ImageFormat.jpg
			var thumb = await new Thumbnail(iStorage,
				iStorage, new FakeIWebLogger()).ResizeThumbnailFromSourceImage(
				newImage.FullFilePath, 1, null, true);
			Assert.AreEqual(true,thumb.CanRead);
		}
        
		[TestMethod]
		public async Task ResizeThumbnailToStream__PNG_Test()
		{
			var thumb = await new Thumbnail(_iStorage,
				_iStorage, new FakeIWebLogger()).ResizeThumbnailFromSourceImage(
				_fakeIStorageImageSubPath, 1, null, true,
				ExtensionRolesHelper.ImageFormat.png);
			Assert.AreEqual(true,thumb.CanRead);
		}
		
		[TestMethod]
		public async Task ResizeThumbnailToStream_CorruptImage()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = await new Thumbnail(storage, 
				storage,
				new FakeIWebLogger()).ResizeThumbnailFromSourceImage("test",1);
			Assert.IsNull(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ResizeThumbnailImageFormat_NullInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			await new Thumbnail(storage, 
				storage, new FakeIWebLogger()).
				SaveThumbnailImageFormat(null,ExtensionRolesHelper.ImageFormat.bmp, null);
			// ArgumentNullException
		}
		
		[TestMethod]
		public void RemoveCorruptImage_RemoveCorruptImage()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = new Thumbnail(storage, 
				storage, new FakeIWebLogger()).RemoveCorruptImage("test");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void RemoveCorruptImage_ShouldIgnore()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {CreateAnImage.Bytes});

			var result = new Thumbnail(
				storage, storage, 
				new FakeIWebLogger()).RemoveCorruptImage("test");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void RemoveCorruptImage_NotExist()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> (), 
				new List<byte[]> {CreateAnImage.Bytes});

			var result = new Thumbnail(storage, 
				storage, new FakeIWebLogger()).RemoveCorruptImage("test");
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RotateThumbnail_NotFound()
		{
			var result = new Thumbnail(_iStorage, 
				_iStorage, new FakeIWebLogger())
				.RotateThumbnail("not-found",0, 3);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RotateThumbnail_Rotate()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"/test.jpg"}, 
				new List<byte[]> {CreateAnImage.Bytes});
			
			var result = new Thumbnail(storage, 
				storage, new FakeIWebLogger())
				.RotateThumbnail("/test.jpg",-1, 3);
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void RotateThumbnail_Corrupt()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = new Thumbnail(storage, 
					storage, new FakeIWebLogger()).
				RotateThumbnail("test", 1);
			Assert.IsFalse(result);
		}
		
	}
}
