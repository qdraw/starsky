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
		public async Task Json_Write()
		{
			var fakeStorage = new FakeIStorage();
			await new FileIndexItemJsonParser(fakeStorage).Write(new FileIndexItem("/test.jpg"));
			Assert.IsTrue(fakeStorage.ExistFile("/.starsky.test.jpg.json"));
		}
	}
}
