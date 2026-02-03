using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Helpers;
using starsky.feature.webftppublish.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Helpers;

[TestClass]
public class WebFtpCliPublishableTests
{
	private WebFtpCli CreateWebFtpCli(AppSettings appSettings,
		FakeIPublishPreflight publishPreflight)
	{
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var console = new FakeConsoleWithCapture();
		var ftpFactory = new FakeFtpWebRequestFactory();

		return new WebFtpCli(appSettings, selectorStorage, console, ftpFactory,
			publishPreflight);
	}

	[TestMethod]
	public async Task RunAsync_PublishableProfile_Succeeds()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			WebFtp = "ftp://test",
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"publishable",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = true } }
				}
			}
		};

		var publishPreflight = new FakeIPublishPreflight();
		publishPreflight.SetPublishable("publishable", true);

		var cli = CreateWebFtpCli(appSettings, publishPreflight);

		var tempDir = Path.GetTempPath();
		var testDir = Path.Combine(tempDir, "test_publish");
		Directory.CreateDirectory(testDir);

		var manifest = new FtpPublishManifestModel
		{
			Slug = "test",
			Copy = new Dictionary<string, bool> { { "test.html", true } },
			PublishProfileName = "publishable"
		};

		var settingsFile = Path.Combine(testDir, "_settings.json");
		var json = JsonSerializer.Serialize(manifest);
		await File.WriteAllTextAsync(settingsFile, json);

		// Act
		await cli.RunAsync(new[] { "-p", testDir });

		// Assert - if we get here without exception, validation passed
		Assert.IsTrue(true);

		// Cleanup
		Directory.Delete(testDir, true);
	}

	[TestMethod]
	public async Task RunAsync_NonPublishableProfile_FailsWithError()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			WebFtp = "ftp://test",
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"not_publishable",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = false } }
				}
			}
		};

		var publishPreflight = new FakeIPublishPreflight();
		publishPreflight.SetPublishable("not_publishable", false);

		var cli = CreateWebFtpCli(appSettings, publishPreflight);

		var tempDir = Path.GetTempPath();
		var testDir = Path.Combine(tempDir, "test_publish_fail");
		Directory.CreateDirectory(testDir);

		var manifest = new FtpPublishManifestModel
		{
			Slug = "test",
			Copy = new Dictionary<string, bool> { { "test.html", true } },
			PublishProfileName = "not_publishable"
		};

		var settingsFile = Path.Combine(testDir, "_settings.json");
		var json = JsonSerializer.Serialize(manifest);
		await File.WriteAllTextAsync(settingsFile, json);

		// Act
		var console = ( FakeConsoleWithCapture )
			System.Reflection.typeof(WebFtpCli)
			.GetField("_console", BindingFlags.NonPublic |
			                      BindingFlags.Instance)
			?.GetValue(cli) ?? new FakeConsoleWithCapture();

		await cli.RunAsync(new[] { "-p", testDir });

		// Assert
		Assert.IsTrue(console.LastOutput.Contains("not allowed to publish"));

		// Cleanup
		Directory.Delete(testDir, true);
	}

	private class FakeConsoleWithCapture : IConsole
	{
		public string LastOutput { get; set; } = string.Empty;

		public void Write(string value)
		{
			LastOutput += value;
		}

		public void WriteLine(string value)
		{
			LastOutput += value + "\n";
		}

		public string ReadLine()
		{
			return string.Empty;
		}
	}
}
