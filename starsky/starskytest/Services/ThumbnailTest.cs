using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Services;
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
			_exifTool = new FakeExifTool();
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
			var isCreated = new Thumbnail(_iStorage, _exifTool, _readMeta).CreateThumb("/notfound.jpg", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_WrongImageType()
		{
			var isCreated = new Thumbnail(_iStorage, _exifTool,_readMeta).CreateThumb("/notfound.dng", _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_ThumbnailAlreadyExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_fakeIStorageImageSubPath}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, 
				new List<string>{"/test.jpg"});

			var isCreated = new Thumbnail(storage, _exifTool,_readMeta).CreateThumb(_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
			Assert.AreEqual(false,isCreated);
		}
		
		[TestMethod]
		public void CreateThumbTest_WriteToMemory()
		{
			var isCreated = new Thumbnail(_iStorage, _exifTool,_readMeta).CreateThumb(_fakeIStorageImageSubPath, _fakeIStorageImageSubPath);
			Assert.AreEqual(true,isCreated);
		}
	}
}
