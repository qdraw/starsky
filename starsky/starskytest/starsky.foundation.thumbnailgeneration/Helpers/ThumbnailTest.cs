using System;
using System.Collections.Generic;
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
		public void CreateThumbTest_FileHash_FileHashNull()
		{
			new Thumbnail(_iStorage, _iStorage).CreateThumb(
				"/notfound.jpg", null);
			// expect ArgumentNullException
		}

		[TestMethod]
		public void CreateThumbTest_FileHash_ImageSubPathNotFound()
		{
			var isCreated = new Thumbnail(_iStorage, _iStorage).CreateThumb(
				"/notfound.jpg", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_FileHash_WrongImageType()
		{
			var isCreated = new Thumbnail(_iStorage, _iStorage).CreateThumb(
				"/notfound.dng", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_FileHash_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});

			var isCreated = new Thumbnail(storage, storage).CreateThumb(
				_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}

		[TestMethod]
		public void CreateThumbTest_1arg_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var isCreated = new Thumbnail(storage, storage).CreateThumb(
				_fakeIStorageImageSubPath);
			Assert.AreEqual(true,isCreated);
		}

		[TestMethod]
		public void ResizeThumbnailToStream__HostDependency__JPEG_Test()
		{
			var newImage = new CreateAnImage();
			var iStorage = new StorageHostFullPathFilesystem();

			// string subPath, int width, string outputHash = null,bool removeExif = false,ExtensionRolesHelper.ImageFormat
			// imageFormat = ExtensionRolesHelper.ImageFormat.jpg
			var thumb = new Thumbnail(iStorage,iStorage).ResizeThumbnail(
				newImage.FullFilePath, 1, null, true);
			Assert.AreEqual(true,thumb.CanRead);
		}
        
		[TestMethod]
		public void ResizeThumbnailToStream__PNG_Test()
		{
			var thumb = new Thumbnail(_iStorage,_iStorage).ResizeThumbnail(
				_fakeIStorageImageSubPath, 1, null, true,
				ExtensionRolesHelper.ImageFormat.png);
			Assert.AreEqual(true,thumb.CanRead);
		}
		
		[TestMethod]
		public void ResizeThumbnailToStream_CorruptImage()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = new Thumbnail(storage, 
				storage).ResizeThumbnail("test",1);
			Assert.IsNull(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ResizeThumbnailImageFormat_NullInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			new Thumbnail(storage, 
				storage).ResizeThumbnailImageFormat(null,ExtensionRolesHelper.ImageFormat.bmp, null);
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
				storage).RemoveCorruptImage("test");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void RemoveCorruptImage_ShouldIgnore()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {CreateAnImage.Bytes});

			var result = new Thumbnail(storage, storage).RemoveCorruptImage("test");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void RemoveCorruptImage_NotExist()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> (), 
				new List<byte[]> {CreateAnImage.Bytes});

			var result = new Thumbnail(storage, storage).RemoveCorruptImage("test");
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RotateThumbnail_NotFound()
		{
			var result = new Thumbnail(_iStorage, _iStorage).RotateThumbnail("not-found",0, 3);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void RotateThumbnail_Rotate()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"/test.jpg"}, 
				new List<byte[]> {CreateAnImage.Bytes});
			
			var result = new Thumbnail(storage, storage).RotateThumbnail("/test.jpg",-1, 3);
			
			Assert.IsTrue(result);
		}
		
	}
}
