using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.writemeta.JsonService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.JsonService
{
	[TestClass]
	public class FileIndexItemJsonWriterTest
	{
		[TestMethod]
		public async Task Write()
		{
			var fakeStorage = new FakeIStorage();
			await new FileIndexItemJsonWriter(fakeStorage).Write(new FileIndexItem("/test.jpg"));
			Assert.IsTrue(fakeStorage.ExistFile("/._test.json"));
		}
	}
}
