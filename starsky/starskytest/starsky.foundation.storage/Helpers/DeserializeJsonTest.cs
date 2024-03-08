using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Helpers
{
	[TestClass]
	public sealed class DeserializeJsonTest
	{
		[TestMethod]
		public async Task ReadTest_FromCopiedText_T_Model()
		{
			var input =
				"{\n  \"Title\": \"Title\",\n  \"Price\": 200,\n  \"ShowButtons\": " +
				"true}";
			var fakeStorage = new FakeIStorage();
			await fakeStorage.WriteStreamAsync(
				StringToStreamHelper.StringToStream(input), "/test.json");
			
			var itemJsonParser = new DeserializeJson(fakeStorage);

			var result = await itemJsonParser.ReadAsync<FileIndexItemJsonParserTest_TestModel>("/test.json");
			
			Assert.IsNotNull(result);
			Assert.AreEqual(200, result.Price);
			Assert.AreEqual("Title", result.Title);
			Assert.IsTrue(result.ShowButtons);
		}

		[TestMethod]
		public async Task Item()
		{
			const string input = "{  \"$id\": \"https://docs.qdraw.nl/schema/meta-data-container.json\",  " +
			                     "\"$schema\": \"https://json-schema.org/draft/2020-12/schema\",  \"item\": {    " +
			                     "\"filePath\": \"/test.jpg\",    \"fileName\": \"test.jpg\",    \"fileHash\": \"Test\",   " +
			                     " \"fileCollectionName\": \"test\",    \"parentDirectory\": \"/\",    \"isDirectory\": false,   " +
			                     " \"tags\": \"test\",    \"status\": \"ExifWriteNotSupported\",    \"description\": \"Description\",    " +
			                     "\"title\": \"Title\",    \"dateTime\": \"2020-01-01T00:00:00\",    \"addToDatabase\": \"2020-01-01T00:00:00\",    " +
			                     "\"lastEdited\": \"2020-01-01T00:00:00\",    \"latitude\": 50,    \"longitude\": 5,    \"locationAltitude\": 1,   " +
			                     " \"locationCity\": \"LocationCity\",    " +
			                     "\"locationState\": \"LocationState\",    \"locationCountry\": \"LocationCountry\",    " +
			                     "\"locationCountryCode\": null,    \"colorClass\": 2,    \"orientation\": \"Rotate180\",    " +
			                     "\"imageWidth\": 100,    \"imageHeight\": 140,    \"imageFormat\": \"jpg\",    " +
			                     "\"collectionPaths\": [],    \"sidecarExtensionsList\": [],    \"aperture\": 1,    " +
			                     "\"shutterSpeed\": \"10\",    \"isoSpeed\": 1200,    \"software\": \"starsky\",    " +
			                     "\"makeModel\": \"test|test|\",    \"make\": \"test\",    \"model\": \"test\",    " +
			                     "\"lensModel\": \"\",    \"focalLength\": 200,    \"size\": 0,    " +
			                     "\"imageStabilisation\": \"Unknown\",    \"lastChanged\": []  }}";
			var fakeStorage = new FakeIStorage();
			await fakeStorage.WriteStreamAsync(
				StringToStreamHelper.StringToStream(input), "/test.json");
			
			var itemJsonParser = new DeserializeJson(fakeStorage);
			var result = await itemJsonParser.ReadAsync<MetadataContainer>("/test.json");

			Assert.AreEqual("/test.jpg", result?.Item?.FilePath);
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task ReadTest_NotFound()
		{
			var fakeStorage = new FakeIStorage();
			var itemJsonParser = new DeserializeJson(fakeStorage);

			await itemJsonParser.ReadAsync<FileIndexItemJsonParserTest_TestModel>("/notfound.json");
			// expect error
		}
	}

	// ReSharper disable once InconsistentNaming
	// ReSharper disable once ClassNeverInstantiated.Global
	public class FileIndexItemJsonParserTest_TestModel
	{
		public string Title { get; set; } = string.Empty;
		public int Price { get; set; }
		public bool ShowButtons { get; set; }
	}
}
