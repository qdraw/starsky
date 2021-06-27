using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metathumbnail.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services
{
	[TestClass]
	public class WriteMetaThumbnailServiceTest
	{
		[TestMethod]
		public async Task WriteAndCropFile_Fail_BufferNull()
		{
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {"/test.jpg"}, new byte[0][]);
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
				new FakeIWebLogger(), new AppSettings());
			var result = await service.WriteAndCropFile("/test.jpg", new OffsetModel(), 0, 0,
				FileIndexItem.Rotation.Horizontal);
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task WriteAndCropFile_Fail_ImageCantBeLoaded()
		{
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {"/test.jpg"}, new byte[0][]);
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
				new FakeIWebLogger(), new AppSettings());
			var result = await service.WriteAndCropFile("/test.jpg", new OffsetModel
				{
					Data = new byte[10]
				}, 0, 0,
				FileIndexItem.Rotation.Horizontal);
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task WriteAndCropFile_FileIsWritten()
		{
			var storage = new FakeIStorage();
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
				new FakeIWebLogger(), new AppSettings());
			var result = await service.WriteAndCropFile("test", new OffsetModel
					{
						Count = CreateAnImage.Bytes.Length,
						Data =  CreateAnImage.Bytes,
						Index = 0
					}, 6, 6,
				FileIndexItem.Rotation.Horizontal);
			
			Assert.IsTrue(result);
			Assert.IsTrue(storage.ExistFile(ThumbnailNameHelper.Combine("test",ThumbnailSize.TinyMeta)));
		}

		[TestMethod]
		public void RotateEnumToDegrees_Horizontal()
		{
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(),
				new FakeIWebLogger(), new AppSettings());

			var result = service.RotateEnumToDegrees(FileIndexItem.Rotation.Horizontal);
			Assert.AreEqual(0,result,0.00001);
		}
		
		[TestMethod]
		public void RotateEnumToDegrees_Default()
		{
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(),
				new FakeIWebLogger(), new AppSettings());

			var result = service.RotateEnumToDegrees(FileIndexItem.Rotation.DoNotChange);
			Assert.AreEqual(0,result,0.00001);
		}

		[TestMethod]
		public void RotateEnumToDegrees_180()
		{
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(),
				new FakeIWebLogger(), new AppSettings());

			var result = service.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate180);
			Assert.AreEqual(180,result,0.00001);
		}
		
		[TestMethod]
		public void RotateEnumToDegrees_90()
		{
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(),
				new FakeIWebLogger(), new AppSettings());

			var result = service.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate90Cw);
			Assert.AreEqual(90,result,0.00001);
		}
		
		[TestMethod]
		public void RotateEnumToDegrees_270()
		{
			var service = new WriteMetaThumbnailService(new FakeSelectorStorage(),
				new FakeIWebLogger(), new AppSettings());

			var result = service.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate270Cw);
			Assert.AreEqual(270,result,0.00001);
		}
	}
}
