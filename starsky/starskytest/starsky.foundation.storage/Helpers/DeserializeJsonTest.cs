using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Helpers
{
	[TestClass]
	public class DeserializeJsonTest
	{
		[TestMethod]
		public void ReadTest_FromCopiedText_T_Model()
		{
			var input =
				"{\n  \"Title\": \"Title\",\n  \"Price\": 200,\n  \"ShowButtons\": " +
				"true}";
			var fakeStorage = new FakeIStorage();
			fakeStorage.WriteStream(
				new PlainTextFileHelper().StringToStream(input), "/test.json");
			
			var itemJsonParser = new DeserializeJson(fakeStorage);

			var result = itemJsonParser.Read<FileIndexItemJsonParserTest_TestModel>("/test.json");
			
			Assert.AreEqual(200, result.Price);
			Assert.AreEqual("Title", result.Title);
			Assert.AreEqual(true, result.ShowButtons);
		}
		
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public void ReadTest_NotFound()
		{
			var fakeStorage = new FakeIStorage();
			var itemJsonParser = new DeserializeJson(fakeStorage);

			itemJsonParser.Read<FileIndexItemJsonParserTest_TestModel>("/notfound.json");
			// expect error
		}
	}

	public class FileIndexItemJsonParserTest_TestModel
	{
		public string Title { get; set; }
		public int Price { get; set; }
		public bool ShowButtons { get; set; }
	}
}
