using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Services
{
	[TestClass]
	public class ThumbnailTest
	{
		private FakeExifTool _exifTool;
		private FakeIStorage _iStorage;
		private string _fakeIStorageImageSubPath;
		private ReadMeta _readMeta;

		public ThumbnailTest()
		{
			_exifTool = new FakeExifTool(_iStorage,new AppSettings());
			_fakeIStorageImageSubPath = "/test.jpg";
			

			
			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, 
				new List<string>{null});
			
			_readMeta = new ReadMeta(_iStorage);
		}

		[TestMethod]
		public void CreateThumbTest_ImageSubPathNotFound()
		{
			var isCreated = new Thumbnail(_iStorage).CreateThumb("/notfound.jpg", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_WrongImageType()
		{
			var isCreated = new Thumbnail(_iStorage).CreateThumb("/notfound.dng", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, 
				new List<string>{"/test.jpg"});

			var isCreated = new Thumbnail(storage).CreateThumb(_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
//		[TestMethod]
//		public void CreateThumbTest_WriteToMemory()
//		{
//			var isCreated = new Thumbnail(_iStorage).CreateThumb(_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
//			Assert.AreEqual(true,isCreated);
//		}
		
		
		[TestMethod]
        public void Thumbnail_ResizeThumbnailToStream__HostDependecy__JPEG_Test()
        {

            var newImage = new CreateAnImage();
	        var iStorage = new StorageHostFullPathFilesystem();

	        // string subPath, int width, string outputHash = null,bool removeExif = false,ExtensionRolesHelper.ImageFormat
	        // imageFormat = ExtensionRolesHelper.ImageFormat.jpg
            var thumb = new Thumbnail(iStorage).ResizeThumbnail(newImage.FullFilePath, 1, null, true,
	            ExtensionRolesHelper.ImageFormat.jpg);
            Assert.AreEqual(true,thumb.CanRead);
        }
        
        [TestMethod]
        public void Thumbnail_ResizeThumbnailToStream__PNG_Test()
        {
	        var thumb = new Thumbnail(_iStorage).ResizeThumbnail(_fakeIStorageImageSubPath, 1, null, true,
		        ExtensionRolesHelper.ImageFormat.png);
            Assert.AreEqual(true,thumb.CanRead);
        }
		
		
	}
}
