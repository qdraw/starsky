using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.JsonService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.JsonService
{
	[TestClass]
	public class FileIndexItemJsonParserTest
	{
		[TestMethod]
		public async Task Json_Write()
		{
			var fakeStorage = new FakeIStorage();
			await new FileIndexItemJsonParser(fakeStorage).Write(new FileIndexItem("/test.jpg"));
			Assert.IsTrue(fakeStorage.ExistFile("/.starsky.test.jpg.json"));
		}

		private FileIndexItem ExampleItem { get; set; } = new FileIndexItem("/test.jpg")
		{
			Tags = "test",
			FileHash = "Test",
			IsDirectory = false,
			Status = FileIndexItem.ExifStatus.ExifWriteNotSupported,
			Description = "Description",
			Title = "Title",
			DateTime = new DateTime(2020, 01, 01),
			AddToDatabase = new DateTime(2020, 01, 01),
			LastEdited = new DateTime(2020, 01, 01),
			Latitude = 50,
			Longitude = 5,
			LocationAltitude = 1,
			LocationCity = "LocationCity",
			LocationCountry = "LocationCountry",
			LocationState = "LocationState",
			ColorClass = ColorClassParser.Color.WinnerAlt,
			Orientation = FileIndexItem.Rotation.Rotate180,
			ImageWidth = 100,
			ImageHeight = 140,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
			Aperture = 1,
			ShutterSpeed = "10",
			IsoSpeed = 1200,
			Software = "starsky",
			MakeModel = "test|test|",
			FocalLength = 200,
		};

		[TestMethod]
		public async Task Json_Write_Read()
		{
			var fakeStorage = new FakeIStorage();
			var itemJsonParser = new FileIndexItemJsonParser(fakeStorage);
			await itemJsonParser.Write(ExampleItem);

			var result = itemJsonParser.Read(new FileIndexItem("/test.jpg"));

			Assert.AreEqual(ExampleItem.Tags, result.Tags);
			Assert.AreEqual(ExampleItem.FileHash, result.FileHash);
			Assert.AreEqual(ExampleItem.IsDirectory, result.IsDirectory);
			Assert.AreEqual(ExampleItem.Description, result.Description);
			Assert.AreEqual(ExampleItem.Title, result.Title);
			Assert.AreEqual(ExampleItem.DateTime, result.DateTime);
			Assert.AreEqual(ExampleItem.AddToDatabase, result.AddToDatabase);
			Assert.AreEqual(ExampleItem.LastEdited, result.LastEdited);
			Assert.AreEqual(ExampleItem.Latitude, result.Latitude);
			Assert.AreEqual(ExampleItem.Longitude, result.Longitude);
			Assert.AreEqual(ExampleItem.LocationAltitude, result.LocationAltitude);
			Assert.AreEqual(ExampleItem.LocationCity, result.LocationCity);
			Assert.AreEqual(ExampleItem.LocationCountry, result.LocationCountry);
			Assert.AreEqual(ExampleItem.LocationState, result.LocationState);
			Assert.AreEqual(ExampleItem.ColorClass, result.ColorClass);
			Assert.AreEqual(ExampleItem.Orientation, result.Orientation);
			Assert.AreEqual(ExampleItem.ImageWidth, result.ImageWidth);
			Assert.AreEqual(ExampleItem.ImageHeight, result.ImageHeight);
			Assert.AreEqual(ExampleItem.ImageFormat, result.ImageFormat);
			Assert.AreEqual(ExampleItem.Aperture, result.Aperture);
			Assert.AreEqual(ExampleItem.ShutterSpeed, result.ShutterSpeed);
			Assert.AreEqual(ExampleItem.IsoSpeed, result.IsoSpeed);
			Assert.AreEqual(ExampleItem.Software, result.Software);
			Assert.AreEqual(ExampleItem.MakeModel, result.MakeModel);
			Assert.AreEqual(ExampleItem.FocalLength, result.FocalLength);
		}

		[TestMethod]
		public void ReadTest_FromCopiedText()
		{
			var input =
				"{\n  \"FilePath\": \"/test.jpg\",\n  \"FileName\": \"test.jpg\",\n  \"FileHash\": " +
				"\"Test\",\n  \"FileCollectionName\": \"test\",\n  \"ParentDirectory\": \"/\",\n  " +
				"\"IsDirectory\": false,\n  \"Tags\": \"test\",\n  \"Status\": \"ExifWriteNotSupported\"," +
				"\n  \"Description\": \"Description\",\n  \"Title\": \"Title\",\n  \"DateTime\": " +
				"\"2020-01-01T00:00:00\",\n  \"AddToDatabase\": \"2020-01-01T00:00:00\",\n  \"LastEdited\": " +
				"\"2020-01-01T00:00:00\",\n  \"Latitude\": 50,\n  \"Longitude\": 5,\n  \"LocationAltitude\": 1,\n " +
				" \"LocationCity\": \"LocationCity\",\n  \"LocationState\": \"LocationState\",\n " +
				" \"LocationCountry\": \"LocationCountry\",\n  \"ColorClass\": 2,\n  \"Orientation\": \"Rotate180\",\n " +
				" \"ImageWidth\": 100,\n  \"ImageHeight\": 140,\n  \"ImageFormat\": \"jpg\",\n  \"CollectionPaths\": [],\n" +
				"  \"Aperture\": 1,\n  \"ShutterSpeed\": \"10\",\n  \"IsoSpeed\": 1200,\n  \"Software\": \"starsky\",\n " +
				" \"MakeModel\": \"test|test|\",\n  \"Make\": \"test\",\n  \"Model\": \"test\",\n  \"FocalLength\": 200\n}";

			var fakeStorage = new FakeIStorage();
			var jsonSubPath = "/.starsky." + "test.jpg" + ".json";

			fakeStorage.WriteStream(
				new PlainTextFileHelper().StringToStream(input), jsonSubPath);

			var itemJsonParser = new FileIndexItemJsonParser(fakeStorage);

			var result = itemJsonParser.Read(new FileIndexItem("/test.jpg"));

			Assert.AreEqual(ExampleItem.Tags, result.Tags);
			Assert.AreEqual(ExampleItem.FileHash, result.FileHash);
			Assert.AreEqual(ExampleItem.IsDirectory, result.IsDirectory);
			Assert.AreEqual(ExampleItem.Description, result.Description);
			Assert.AreEqual(ExampleItem.Title, result.Title);
			Assert.AreEqual(ExampleItem.DateTime, result.DateTime);
			Assert.AreEqual(ExampleItem.AddToDatabase, result.AddToDatabase);
			Assert.AreEqual(ExampleItem.LastEdited, result.LastEdited);
			Assert.AreEqual(ExampleItem.Latitude, result.Latitude);
			Assert.AreEqual(ExampleItem.Longitude, result.Longitude);
			Assert.AreEqual(ExampleItem.LocationAltitude, result.LocationAltitude);
			Assert.AreEqual(ExampleItem.LocationCity, result.LocationCity);
			Assert.AreEqual(ExampleItem.LocationCountry, result.LocationCountry);
			Assert.AreEqual(ExampleItem.LocationState, result.LocationState);
			Assert.AreEqual(ExampleItem.ColorClass, result.ColorClass);
			Assert.AreEqual(ExampleItem.Orientation, result.Orientation);
			Assert.AreEqual(ExampleItem.ImageWidth, result.ImageWidth);
			Assert.AreEqual(ExampleItem.ImageHeight, result.ImageHeight);
			Assert.AreEqual(ExampleItem.ImageFormat, result.ImageFormat);
			Assert.AreEqual(ExampleItem.Aperture, result.Aperture);
			Assert.AreEqual(ExampleItem.ShutterSpeed, result.ShutterSpeed);
			Assert.AreEqual(ExampleItem.IsoSpeed, result.IsoSpeed);
			Assert.AreEqual(ExampleItem.Software, result.Software);
			Assert.AreEqual(ExampleItem.MakeModel, result.MakeModel);
			Assert.AreEqual(ExampleItem.FocalLength, result.FocalLength);
		}
	}

}
