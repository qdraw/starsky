using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsDefaultEditorApplicationTest
{
	[TestMethod]
	public void Json_CompareOutputOfString()
	{
		// Create an instance of MyClass
		var myClass = new AppSettingsDefaultEditorApplication
		{
			ImageFormats =
				[ExtensionRolesHelper.ImageFormat.bmp],
			ApplicationPath = @"C:\Program Files\MyApp\MyApp.exe"
		};

		// Serialize the object to JSON
		var json = JsonSerializer.Serialize(myClass, DefaultJsonSerializer.CamelCaseNoEnters);

		const string expected = "{\"imageFormats\":[\"bmp\"]," +
		                        "\"applicationPath\":\"C:\\\\Program Files\\\\MyApp\\\\MyApp.exe\"}";
		Assert.AreEqual(expected, json);
	}

	[TestMethod]
	public void Json_CompareInputOfString()
	{
		// Create an instance of MyClass
		const string input = "{\"imageFormats\":[\"bmp\"]," +
		                     "\"applicationPath\":\"C:\\\\Program Files\\\\MyApp\\\\MyApp.exe\"}";

		// Serialize the object to JSON
		var json = JsonSerializer.Deserialize<AppSettingsDefaultEditorApplication>(input,
			DefaultJsonSerializer.CamelCaseNoEnters);

		Assert.IsNotNull(json);
		Assert.AreEqual(@"C:\Program Files\MyApp\MyApp.exe", json.ApplicationPath);
		Assert.AreEqual(1, json.ImageFormats.Count);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, json.ImageFormats[0]);
	}
}
