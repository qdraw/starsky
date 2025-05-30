using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.JsonService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.JsonService;

[TestClass]
public sealed class FileIndexItemJsonParserTest
{
	private FileIndexItem ExampleItem { get; } = new("/test.jpg")
	{
		Tags = "test",
		FileHash = "Test",
		IsDirectory = false,
		Status = FileIndexItem.ExifStatus.ExifWriteNotSupported,
		Description = "Description",
		Title = "Title",
		DateTime = new DateTime(2020, 01, 01, 00, 00, 00, DateTimeKind.Unspecified),
		AddToDatabase = new DateTime(2020, 01, 01, 00, 00, 00, DateTimeKind.Local),
		LastEdited = new DateTime(2020, 01, 01, 00, 00, 00, DateTimeKind.Local),
		Latitude = 50,
		Longitude = 5,
		LocationAltitude = 1,
		LocationCity = "LocationCity",
		LocationCountry = "LocationCountry",
		LocationState = "LocationState",
		ColorClass = ColorClassParser.Color.WinnerAlt,
		Orientation = ImageRotation.Rotation.Rotate180,
		ImageWidth = 100,
		ImageHeight = 140,
		ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
		Aperture = 1,
		ShutterSpeed = "10",
		IsoSpeed = 1200,
		Software = "starsky",
		MakeModel = "test|test|",
		FocalLength = 200
	};

	[TestMethod]
	public async Task Json_Write()
	{
		var fakeStorage = new FakeIStorage();
		await new FileIndexItemJsonParser(fakeStorage).WriteAsync(new FileIndexItem("/test.jpg"));
		Assert.IsTrue(fakeStorage.ExistFile("/.starsky.test.jpg.json"));
	}

	[TestMethod]
	public async Task Json_Write_ImageFormat_Read()
	{
		var fakeStorage = new FakeIStorage();
		await new FileIndexItemJsonParser(fakeStorage).WriteAsync(new FileIndexItem("/test.jpg"));
		var jsonSubPath = JsonSidecarLocation.JsonLocation("/", "test.jpg");

		var stream = fakeStorage.ReadStream(jsonSubPath, 67) as MemoryStream;
		var byteArray = stream!.ToArray().Take(67).ToArray();
		Console.WriteLine(string.Join(", ", byteArray));
		Console.WriteLine(BitConverter.ToString(byteArray).Replace("-", string.Empty));

		var imageFormat = new ExtensionRolesHelper(new FakeIWebLogger()).GetImageFormat(
			fakeStorage.ReadStream(jsonSubPath, 67));

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.meta_json, imageFormat);
	}

	[TestMethod]
	public async Task Json_Write_Read()
	{
		var fakeStorage = new FakeIStorage();
		var itemJsonParser = new FileIndexItemJsonParser(fakeStorage);
		await itemJsonParser.WriteAsync(ExampleItem);

		var result = await itemJsonParser.ReadAsync(new FileIndexItem("/test.jpg"));

		Assert.IsNotNull(result);
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
	public async Task ReadTest_OldFormat_Unsupported()
	{
		const string input = "{\n  \"FilePath\": \"/test.jpg\",\n " +
		                     " \"FileName\": \"test.jpg\",\n  \"FileHash\": " +
		                     "\"Test\",\n  \"FileCollectionName\": \"test\",\n  " +
		                     "\"ParentDirectory\": \"/\",\n  " +
		                     "\"IsDirectory\": false,\n  \"Tags\": \"test\",\n  " +
		                     "\"Status\": \"ExifWriteNotSupported\"," +
		                     "\n  \"Description\": \"Description\",\n  " +
		                     "\"Title\": \"Title\",\n  \"DateTime\": " +
		                     "\"2020-01-01T00:00:00\",\n  " +
		                     "\"AddToDatabase\": \"2020-01-01T00:00:00\",\n  \"LastEdited\": " +
		                     "\"2020-01-01T00:00:00\",\n  \"Latitude\": 50,\n  " +
		                     "\"Longitude\": 5,\n  \"LocationAltitude\": 1,\n " +
		                     " \"LocationCity\": \"LocationCity\",\n  " +
		                     "\"LocationState\": \"LocationState\",\n " +
		                     " \"LocationCountry\": \"LocationCountry\",\n  " +
		                     "\"ColorClass\": 2,\n  \"Orientation\": \"Rotate180\",\n " +
		                     " \"ImageWidth\": 100,\n  \"ImageHeight\": 140,\n  " +
		                     "\"ImageFormat\": \"jpg\",\n  \"CollectionPaths\": [],\n" +
		                     "  \"Aperture\": 1,\n  \"ShutterSpeed\": \"10\",\n  " +
		                     "\"IsoSpeed\": 1200,\n  \"Software\": \"starsky\",\n " +
		                     " \"MakeModel\": \"test|test|\",\n  \"Make\": \"test\",\n  " +
		                     "\"Model\": \"test\",\n  \"FocalLength\": 200\n}";
		var fakeStorage = new FakeIStorage();
		var jsonSubPath = "/.starsky." + "test.jpg" + ".json";

		await fakeStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream(input), jsonSubPath);

		var itemJsonParser = new FileIndexItemJsonParser(fakeStorage);

		var result = await itemJsonParser.ReadAsync(new FileIndexItem("/test.jpg"));

		Assert.AreEqual(string.Empty, result.Tags);
	}

	[TestMethod]
	public async Task ReadTest_FromCopiedText()
	{
		const string input = "{ \"item\": {\n  \"FilePath\": \"/test.jpg\",\n " +
		                     " \"FileName\": \"test.jpg\",\n  \"FileHash\": " +
		                     "\"Test\",\n  \"FileCollectionName\": \"test\",\n  " +
		                     "\"ParentDirectory\": \"/\",\n  " +
		                     "\"IsDirectory\": false,\n  \"Tags\": \"test\",\n  " +
		                     "\"Status\": \"ExifWriteNotSupported\"," +
		                     "\n  \"Description\": \"Description\",\n  " +
		                     "\"Title\": \"Title\",\n  \"DateTime\": " +
		                     "\"2020-01-01T00:00:00\",\n  " +
		                     "\"AddToDatabase\": \"2020-01-01T00:00:00\",\n  \"LastEdited\": " +
		                     "\"2020-01-01T00:00:00\",\n  \"Latitude\": 50,\n  " +
		                     "\"Longitude\": 5,\n  \"LocationAltitude\": 1,\n " +
		                     " \"LocationCity\": \"LocationCity\",\n  " +
		                     "\"LocationState\": \"LocationState\",\n " +
		                     " \"LocationCountry\": \"LocationCountry\",\n  " +
		                     "\"ColorClass\": 2,\n  \"Orientation\": \"Rotate180\",\n " +
		                     " \"ImageWidth\": 100,\n  \"ImageHeight\": 140,\n  " +
		                     "\"ImageFormat\": \"jpg\",\n  \"CollectionPaths\": [],\n" +
		                     "  \"Aperture\": 1,\n  \"ShutterSpeed\": \"10\",\n  " +
		                     "\"IsoSpeed\": 1200,\n  \"Software\": \"starsky\",\n " +
		                     " \"MakeModel\": \"test|test|\",\n  \"Make\": \"test\",\n  " +
		                     "\"Model\": \"test\",\n  \"FocalLength\": 200\n}}";

		var fakeStorage = new FakeIStorage();
		var jsonSubPath = "/.starsky." + "test.jpg" + ".json";

		await fakeStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream(input), jsonSubPath);

		var itemJsonParser = new FileIndexItemJsonParser(fakeStorage);

		var result = await itemJsonParser.ReadAsync(new FileIndexItem("/test.jpg"));

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
