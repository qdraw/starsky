using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class ToBase64DataUriListTest
	{
		[TestMethod]
		public void TestIfContainsDataImageBaseHash()
		{
			var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},new List<byte[]>{CreateAnImage.Bytes});
			var result = new ToBase64DataUriList(fakeStorage, fakeStorage).Create(
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			Assert.IsTrue(result[0].Contains("data:image/png;base64,"));
		}
	}
}
