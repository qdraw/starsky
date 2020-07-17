using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public class MetaInfoTest
	{
		[TestMethod]
		public void FileNotInIndex()
		{
			var metaInfo = new MetaInfo(new FakeIQuery(), new AppSettings(),
				new FakeSelectorStorage());
			var test = metaInfo.GetInfo(new List<string>{"/test"}, false);
			Assert.AreEqual(test.FirstOrDefault().Status, FileIndexItem.ExifStatus.NotFoundNotInIndex);
		}
		
		[TestMethod]
		public void NotFoundSourceMissing()
		{
			var metaInfo = new MetaInfo(new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}), new AppSettings(),
				new FakeSelectorStorage());
			var test = metaInfo.GetInfo(new List<string>{"/test"}, false);
			Assert.AreEqual(test.FirstOrDefault().Status, FileIndexItem.ExifStatus.NotFoundSourceMissing);
		}
		
		[TestMethod]
		public void ExtensionNotSupported_OperationNotSupported()
		{
			var metaInfo = new MetaInfo(new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}), new AppSettings(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>(), new List<string> {"/test"})));
			var test = metaInfo.GetInfo(new List<string>{"/test"}, false);
			Assert.AreEqual(test.FirstOrDefault().Status, FileIndexItem.ExifStatus.OperationNotSupported);
		}
	}
}
