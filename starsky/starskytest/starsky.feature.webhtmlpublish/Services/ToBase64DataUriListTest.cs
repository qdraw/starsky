using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public sealed class ToBase64DataUriListTest
	{
		[TestMethod]
		public async Task TestIfContainsDataImageBaseHash()
		{
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{CreateAnImage.Bytes.ToArray()});
			var result = await new ToBase64DataUriList(fakeStorage, 
				fakeStorage, new FakeIWebLogger(), new AppSettings()).Create(
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			Assert.IsTrue(result[0].Contains("data:image/png;base64,"));
		}
		
		[TestMethod]
		public async Task TestIfContainsDataImageBaseHash_CorruptOutput()
		{
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{new CreateAnImageCorrupt().Bytes.ToArray()});
			var result = await new ToBase64DataUriList(fakeStorage, 
				fakeStorage, new FakeIWebLogger(), new AppSettings()).Create(
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			// to fallback image (1px x 1px)
			Assert.AreEqual("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAA" +
			                "C1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=",result[0]);
		}
	}
}
