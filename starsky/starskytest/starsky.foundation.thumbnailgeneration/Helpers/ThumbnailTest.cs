using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Helpers
{
	[TestClass]
	public sealed class ThumbnailTest
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

			await storage.WriteStreamAsync(
				PlainTextFileHelper.StringToStream("not 0 bytes"), 
				ThumbnailNameHelper.Combine(_fakeIStorageImageSubPath, ThumbnailSize.Small));
			
			var isCreated = await new Thumbnail(storage, storage, 
				new FakeIWebLogger()).CreateThumb( _fakeIStorageImageSubPath, 
				_fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_SkipExtraLarge()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});

			var fileHash = "test_hash";
			
			// skip xtra large
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger()).CreateThumb(
				_fakeIStorageImageSubPath, fileHash, true);
			Assert.AreEqual(true,isCreated);

			Assert.AreEqual(true, storage.ExistFile(fileHash));
			Assert.AreEqual(true, storage.ExistFile(
				ThumbnailNameHelper.Combine(fileHash,ThumbnailSize.Small)));
			Assert.AreEqual(false, storage.ExistFile(
				ThumbnailNameHelper.Combine(fileHash,ThumbnailSize.ExtraLarge)));
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_IncludeExtraLarge()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});

			var fileHash = "test_hash";
			// include xtra large
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger()).CreateThumb(
				_fakeIStorageImageSubPath, fileHash);
			Assert.AreEqual(true,isCreated);

			Assert.AreEqual(true, storage.ExistFile(fileHash));
			Assert.AreEqual(true, storage.ExistFile(
				ThumbnailNameHelper.Combine(fileHash,ThumbnailSize.Small)));
			Assert.AreEqual(true, storage.ExistFile(
				ThumbnailNameHelper.Combine(fileHash,ThumbnailSize.ExtraLarge)));
		}

		[TestMethod]
		public async Task CreateThumbTest_1arg_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var hash = (await new FileHash(storage).GetHashCodeAsync(_fakeIStorageImageSubPath)).Key;
			await storage.WriteStreamAsync(
				PlainTextFileHelper.StringToStream("not 0 bytes"), 
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.ExtraLarge));
			await storage.WriteStreamAsync(
				PlainTextFileHelper.StringToStream("not 0 bytes"), 
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.Large));
			await storage.WriteStreamAsync(
				PlainTextFileHelper.StringToStream("not 0 bytes"), 
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.Small));
			
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger()).CreateThumb(
				_fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated[0].Item2);
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
			Assert.AreEqual(true,thumb.Item1.CanRead);
		}
        
		[TestMethod]
		public async Task ResizeThumbnailToStream__PNG_Test()
		{
			var thumb = await new Thumbnail(_iStorage,
				_iStorage, new FakeIWebLogger()).ResizeThumbnailFromSourceImage(
				_fakeIStorageImageSubPath, 1, null, true,
				ExtensionRolesHelper.ImageFormat.png);
			Assert.AreEqual(true,thumb.Item1.CanRead);
		}
		
		[TestMethod]
		public async Task ResizeThumbnailToStream_CorruptImage_MemoryStream()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = (await new Thumbnail(storage, 
				storage,
				new FakeIWebLogger()).ResizeThumbnailFromSourceImage("test",1)).Item1;
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public async Task ResizeThumbnailToStream_CorruptImage_Status()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = (await new Thumbnail(storage, 
				storage,
				new FakeIWebLogger()).ResizeThumbnailFromSourceImage("test",1)).Item2;
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ResizeThumbnailImageFormat_NullInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			await  Thumbnail.SaveThumbnailImageFormat(null,
				ExtensionRolesHelper.ImageFormat.bmp, null);
			// ArgumentNullException
		}
		
		[TestMethod]
		public void RemoveCorruptImage_RemoveCorruptImage()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {ThumbnailNameHelper.Combine("test", ThumbnailSize.ExtraLarge) }, 
				new List<byte[]> {new byte[0]});

			var result = new Thumbnail(storage, 
				storage, new FakeIWebLogger()).RemoveCorruptImage("test", ThumbnailSize.ExtraLarge);
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void RemoveCorruptImage_ShouldIgnore()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {ThumbnailNameHelper.Combine("test", ThumbnailSize.ExtraLarge) }, 
				new List<byte[]> {CreateAnImage.Bytes});

			var result = new Thumbnail(
				storage, storage, 
				new FakeIWebLogger()).RemoveCorruptImage("test", ThumbnailSize.Large);
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
				storage, new FakeIWebLogger()).RemoveCorruptImage("test", ThumbnailSize.Large);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task RotateThumbnail_NotFound()
		{
			var result = await new Thumbnail(_iStorage, 
				_iStorage, new FakeIWebLogger())
				.RotateThumbnail("not-found",0, 3);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task RotateThumbnail_Rotate()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"/test.jpg"}, 
				new List<byte[]> {CreateAnImage.Bytes});
			
			var result = await new Thumbnail(storage, 
				storage, new FakeIWebLogger())
				.RotateThumbnail("/test.jpg",-1, 3);
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task RotateThumbnail_Corrupt()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = await new Thumbnail(storage, 
					storage, new FakeIWebLogger()).
				RotateThumbnail("test", 1);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task ResizeThumbnailFromThumbnailImage_CorruptInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> {new byte[0]});

			var result = await new Thumbnail(storage, 
					storage, new FakeIWebLogger()).
				ResizeThumbnailFromThumbnailImage("test", 1);
			Assert.IsNull(result);
		}
		
	}
}
