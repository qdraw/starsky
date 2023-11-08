using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
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
			await new Thumbnail(_iStorage, _iStorage, new FakeIWebLogger(), new AppSettings()).CreateThumbAsync(
				"/notfound.jpg", null!);
			// expect ArgumentNullException
		}

		[TestMethod]
		public async Task CreateThumbTest_FileHash_ImageSubPathNotFound()
		{
			var isCreated = await new Thumbnail(_iStorage, _iStorage, new FakeIWebLogger(), new AppSettings()).CreateThumbAsync(
				"/notfound.jpg", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated.FirstOrDefault()!.Success);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_WrongImageType()
		{
			var isCreated =  await new Thumbnail(_iStorage, 
				_iStorage, new FakeIWebLogger(), new AppSettings()).CreateThumbAsync(
				"/notfound.dng", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated.FirstOrDefault()!.Success);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_FileHash_AlreadyFailedBefore()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var thumbnailService =  new Thumbnail(storage, storage, 
				new FakeIWebLogger(), new AppSettings());
				
			await thumbnailService.WriteErrorMessageToBlockLog(_fakeIStorageImageSubPath, "fail");
			
			var isCreated = (await thumbnailService.CreateThumbAsync( _fakeIStorageImageSubPath, 
				_fakeIStorageImageSubPath)).ToList();
				
			Assert.AreEqual(false,isCreated!.FirstOrDefault()!.Success);
			Assert.AreEqual("File already failed before",isCreated.FirstOrDefault()!.ErrorMessage);
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
				storage, new FakeIWebLogger(), new AppSettings()).CreateThumbAsync(
				_fakeIStorageImageSubPath, fileHash, true);
			Assert.AreEqual(true,isCreated.FirstOrDefault()!.Success);

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
				storage, new FakeIWebLogger(), new AppSettings()).CreateThumbAsync(
				_fakeIStorageImageSubPath, fileHash);
			Assert.AreEqual(true,isCreated.FirstOrDefault()!.Success);

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
				storage, new FakeIWebLogger(), new AppSettings()).CreateThumbnailAsync(
				_fakeIStorageImageSubPath);
			Assert.AreEqual(true,isCreated[0].Success);
		}

		[TestMethod]
		public async Task CreateThumbTest_1arg_Folder()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger(), new AppSettings()).CreateThumbnailAsync("/");
			Assert.AreEqual(true,isCreated[0].Success);
		}
		
		[TestMethod]
		public async Task CreateThumbTest_NullFail()
		{
			var storage = new FakeIStorage(new List<string>{"/test"}, 
				new List<string>{"/test/test.jpg"}, 
				new List<byte[]>{ null });
			
			var isCreated = await new Thumbnail(storage, 
				storage, new FakeIWebLogger(), new AppSettings()).CreateThumbnailAsync("/test/test.jpg");
			
			Assert.AreEqual(0,isCreated.Count);
		}

		[TestMethod]
		public async Task ResizeThumbnailToStream__HostDependency__JPEG_Test()
		{
			var newImage = new CreateAnImage();
			var iStorage = new StorageHostFullPathFilesystem();

			// string subPath, int width, string outputHash = null,bool removeExif = false,ExtensionRolesHelper.ImageFormat
			// imageFormat = ExtensionRolesHelper.ImageFormat.jpg
			var thumb = await new Thumbnail(iStorage,
				iStorage, new FakeIWebLogger(), new AppSettings()).ResizeThumbnailFromSourceImage(
				newImage.FullFilePath, 1, null, true);
			Assert.AreEqual(true,thumb.Item1.CanRead);
		}
        
		[TestMethod]
		public async Task ResizeThumbnailToStream__PNG_Test()
		{
			var thumb = await new Thumbnail(_iStorage,
				_iStorage, new FakeIWebLogger(), new AppSettings()).ResizeThumbnailFromSourceImage(
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
				new List<byte[]> { Array.Empty<byte>() });

			var result = (await new Thumbnail(storage, 
				storage,
				new FakeIWebLogger(), new AppSettings()).ResizeThumbnailFromSourceImage("test",1)).Item1;
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public async Task ResizeThumbnailToStream_CorruptImage_Status()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> { Array.Empty<byte>() });

			var result = (await new Thumbnail(storage, 
				storage,
				new FakeIWebLogger(), new AppSettings()).ResizeThumbnailFromSourceImage("test",1)).Item2;
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ResizeThumbnailImageFormat_NullInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> { Array.Empty<byte>() });

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
				new List<byte[]> { Array.Empty<byte>() });

			var result = new Thumbnail(storage, 
				storage, new FakeIWebLogger(), new AppSettings()).RemoveCorruptImage("test", ThumbnailSize.ExtraLarge);
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
				new FakeIWebLogger(), new AppSettings()).RemoveCorruptImage("test", ThumbnailSize.Large);
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
				storage, new FakeIWebLogger(), new AppSettings()).RemoveCorruptImage("test", ThumbnailSize.Large);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task RotateThumbnail_NotFound()
		{
			var result = await new Thumbnail(_iStorage, 
				_iStorage, new FakeIWebLogger(), new AppSettings())
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
				storage, new FakeIWebLogger(), new AppSettings())
				.RotateThumbnail("/test.jpg",-1, 3);
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task RotateThumbnail_Corrupt()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> { Array.Empty<byte>() });

			var result = await new Thumbnail(storage, 
					storage, new FakeIWebLogger(), new AppSettings()).
				RotateThumbnail("test", 1);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task ResizeThumbnailFromThumbnailImage_CorruptInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> { Array.Empty<byte>() });

			var result = await new Thumbnail(storage, 
					storage, new FakeIWebLogger(), new AppSettings()).
				ResizeThumbnailFromThumbnailImage("test", 1);
			Assert.IsNull(result.Item1);
			Assert.IsFalse(result.Item2.Success);

		}
		
		[TestMethod]
		public async Task CreateLargestImageFromSource_CorruptInput()
		{
			var storage = new FakeIStorage(
				new List<string> {"/"}, 
				new List<string> {"test"}, 
				new List<byte[]> { Array.Empty<byte>() });

			var result = await new Thumbnail(storage, 
					storage, new FakeIWebLogger(), new AppSettings()).
				CreateLargestImageFromSource("test", "test", "test", ThumbnailSize.Small);
			
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Image cannot be loaded",result.ErrorMessage);
		}
		
	}
}
