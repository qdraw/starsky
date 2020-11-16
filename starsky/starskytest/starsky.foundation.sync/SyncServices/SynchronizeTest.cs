using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SynchronizeTest
	{
		[TestMethod]
		public async Task Sync_NotFound()
		{
			var sync = new Synchronize(new AppSettings(), new FakeIQuery(), new FakeSelectorStorage());
			var result = await sync.Sync("/not_found.jpg");
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Sync_File()
		{
			var sync = new Synchronize(new AppSettings(), new FakeIQuery(), 
				new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"})));
			
			var result = await sync.Sync("/test.jpg");

			// is missing actual bytes
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result.FirstOrDefault().Status);
		}
		
				
		[TestMethod]
		public async Task Sync_Folder()
		{
			var sync = new Synchronize(new AppSettings(), new FakeIQuery(), 
				new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"})));
			
			var result = await sync.Sync("/");

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result.FirstOrDefault().Status);
		}
	}
}
