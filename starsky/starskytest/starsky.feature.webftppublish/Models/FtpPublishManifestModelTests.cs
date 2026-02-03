using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Models;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.feature.webftppublish.Models;

[TestClass]
public class FtpPublishManifestModelTests
{
	[TestMethod]
	public void FtpPublishManifestModel_WithPublishProfileName_SerializesCorrectly()
	{
		// Arrange
		var manifest = new FtpPublishManifestModel
		{
			Slug = "test_item",
			Copy = new Dictionary<string, bool>
			{
				{ "index.html", true }, { "style.css", true }
			},
			PublishProfileName = "production"
		};

		// Act
		var json = JsonSerializer.Serialize(manifest, DefaultJsonSerializer.NoNamingPolicy);
		var deserialized = JsonSerializer.Deserialize<FtpPublishManifestModel>(json,
			DefaultJsonSerializer.NoNamingPolicy);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual("test_item", deserialized.Slug);
		Assert.AreEqual("production", deserialized.PublishProfileName);
		Assert.AreEqual(deserialized.Copy.Count, 2);
	}

	[TestMethod]
	public void FtpPublishManifestModel_WithoutPublishProfileName_DeserializesCorrectly()
	{
		// Arrange
		var json = @"
			{
				""Slug"": ""old_format"",
				""Copy"": {
					""index.html"": true
				}
			}";

		// Act
		var deserialized = JsonSerializer.Deserialize<FtpPublishManifestModel>(json,
			DefaultJsonSerializer.NoNamingPolicy);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual("old_format", deserialized.Slug);
		Assert.IsNull(deserialized.PublishProfileName);
	}

	[TestMethod]
	public void FtpPublishManifestModel_PublishProfileNameNullByDefault()
	{
		// Arrange & Act
		var manifest = new FtpPublishManifestModel();

		// Assert
		Assert.IsNull(manifest.PublishProfileName);
	}

	[TestMethod]
	public void FtpPublishManifestModel_CanSetAndRetrievePublishProfileName()
	{
		// Arrange
		var manifest = new FtpPublishManifestModel { Slug = "test" };

		// Act
		manifest.PublishProfileName = "staging";
		var result = manifest.PublishProfileName;

		// Assert
		Assert.AreEqual("staging", result);
	}

	[TestMethod]
	[DataRow("production")]
	[DataRow("staging")]
	[DataRow("development")]
	[DataRow("_default")]
	public void FtpPublishManifestModel_WithVariousProfileNames(string profileName)
	{
		// Arrange
		var manifest = new FtpPublishManifestModel
		{
			Slug = "test", PublishProfileName = profileName
		};

		// Act
		var json = JsonSerializer.Serialize(manifest, DefaultJsonSerializer.NoNamingPolicy);
		var deserialized = JsonSerializer.Deserialize<FtpPublishManifestModel>(json,
			DefaultJsonSerializer.NoNamingPolicy);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(profileName, deserialized.PublishProfileName);
	}
}
