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
		public async Task WriteAndCropFile_Fail()
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
	}
}
