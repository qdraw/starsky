using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class PublishPreflightTest
{
	private readonly AppSettings _testAppSettings = new()
	{
		PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
		{
			{ "test", new List<AppSettingsPublishProfiles>() }
		}
	};

	[TestMethod]
	public void GetPublishProfileNames_listNoContent()
	{
		var appSettings = new AppSettings();
		var list = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.GetPublishProfileNames();

		Assert.IsEmpty(list);
	}

	[TestMethod]
	public void GetPublishProfileNames_list()
	{
		var list = new PublishPreflight(_testAppSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.GetPublishProfileNames();

		Assert.HasCount(1, list);
		Assert.AreEqual("test", list[0].Item2);
		Assert.AreEqual(0, list[0].Item1);
	}

	[TestMethod]
	public void GetAllPublishProfileNames_item()
	{
		var list = new PublishPreflight(_testAppSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.GetAllPublishProfileNames();

		Assert.AreEqual("test", list.FirstOrDefault().Key);
	}

	[TestMethod]
	public void GetPublishProfileNameByIndex_0()
	{
		var data = new PublishPreflight(_testAppSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.GetPublishProfileNameByIndex(0);
		Assert.AreEqual("test", data);
	}

	[TestMethod]
	public void GetPublishProfileNameByIndex_LargerThanIndex()
	{
		var data = new PublishPreflight(_testAppSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.GetPublishProfileNameByIndex(1);
		Assert.IsNull(data);
	}

	[TestMethod]
	public void GetNameConsole_WithArg()
	{
		var result = new PublishPreflight(_testAppSettings,
			new FakeConsoleWrapper(), new FakeSelectorStorage(),
			new FakeIWebLogger()).GetNameConsole("/", new List<string> { "-n", "t" });

		Assert.AreEqual("t", result);
	}

	[TestMethod]
	public void GetNameConsole_EnterDefaultOption()
	{
		var consoleWrapper = new FakeConsoleWrapper
		{
			LinesToRead = new List<string> { string.Empty }
		};

		var result = new PublishPreflight(_testAppSettings,
				consoleWrapper, new FakeSelectorStorage(), new FakeIWebLogger())
			.GetNameConsole("/test", new List<string>());

		Assert.AreEqual("test", result);
	}

	[TestMethod]
	public void GetNameConsole_UpdateConsoleInput()
	{
		var consoleWrapper = new FakeConsoleWrapper
		{
			LinesToRead = new List<string> { "updated" }
		};

		var result = new PublishPreflight(_testAppSettings,
				consoleWrapper, new FakeSelectorStorage(), new FakeIWebLogger())
			.GetNameConsole("/test", new List<string>());

		Assert.AreEqual("updated", result);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenProfileKeyIsNull()
	{
		// Arrange
		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(null!);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
		Assert.AreEqual("Profile not found", result.Item2[0]);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenProfileValueIsNull()
	{
		// Arrange
		const string publishProfileName = "invalid-profile";

		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(publishProfileName);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
		Assert.AreEqual("Profile not found", result.Item2[0]);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenProfilePathIsEmpty()
	{
		// Arrange
		var publishProfileName = "invalid-profile-2";
		var publishProfile = new AppSettingsPublishProfiles
		{
			Path = string.Empty, ContentType = TemplateContentType.Jpeg
		};
		_testAppSettings.PublishProfiles![publishProfileName] =
			new List<AppSettingsPublishProfiles> { publishProfile };

		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(publishProfileName);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenHtmlTemplateDoesNotExist()
	{
		// Arrange
		const string publishProfileName = "non-existent-template";
		var publishProfile = new AppSettingsPublishProfiles
		{
			Path = "test.jpg",
			ContentType = TemplateContentType.Html,
			Template = "non-existent-template.cshtml"
		};
		_testAppSettings.PublishProfiles![publishProfileName] =
			new List<AppSettingsPublishProfiles> { publishProfile };

		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(publishProfileName);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
		Assert.AreEqual($"View Path {publishProfile.Template} should exists", result.Item2[0]);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenJpegDoesNotExist()
	{
		// Arrange
		const string publishProfileName = "non-exist-jpeg";
		var publishProfile = new AppSettingsPublishProfiles
		{
			Path = "test.jpg", ContentType = TemplateContentType.Jpeg
		};
		_testAppSettings.PublishProfiles![publishProfileName] =
			new List<AppSettingsPublishProfiles> { publishProfile };

		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(publishProfileName);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
		Assert.AreEqual($"Image Path {publishProfile.Path} should exists", result.Item2[0]);
	}

	[TestMethod]
	public void IsProfileValid_ReturnsFalseWithErrorMessage_WhenOnlyFirstJpegDoesNotExist()
	{
		// Arrange
		const string publishProfileName = "non-exist-only-first-jpeg";
		var publishProfile = new AppSettingsPublishProfiles
		{
			Path = "non-exist-only-first-jpeg.jpg",
			ContentType = TemplateContentType.OnlyFirstJpeg
		};
		_testAppSettings.PublishProfiles![publishProfileName] =
			new List<AppSettingsPublishProfiles> { publishProfile };

		var publishPreflight = new PublishPreflight(_testAppSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger());

		// Act
		var result = publishPreflight.IsProfileValid(publishProfileName);

		// Assert
		Assert.IsFalse(result.Item1);
		Assert.HasCount(1, result.Item2);
		Assert.AreEqual($"Image Path {publishProfile.Path} should exists", result.Item2[0]);
	}

	[TestMethod]
	public void IsFtpPublishEnabled_UsesDefaults_WhenNoProfileOverride()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Publishing = new Publishing { Enabled = true }
				},
				PublishTargets = new PublishTargets
				{
					Ftp = new FtpTarget { Enabled = false }
				}
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "test", [new AppSettingsPublishProfiles()] }
			}
		};

		var result = new PublishPreflight(appSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.IsFtpPublishEnabled("test");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsFtpPublishEnabled_ProfileOverrideEnabled_ReturnsTrue()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Publishing = new Publishing { Enabled = true }
				},
				PublishTargets = new PublishTargets
				{
					Ftp = new FtpTarget { Enabled = false }
				}
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test",
					[
						new AppSettingsPublishProfiles
						{
							ProfileFeatures = new ProfileFeatures
							{
								Publishing = new Publishing { Enabled = true }
							},
							PublishTargets = new PublishTargets
							{
								Ftp = new FtpTarget { Enabled = true }
							}
						}
					]
				}
			}
		};

		var result = new PublishPreflight(appSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.IsFtpPublishEnabled("test");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsFtpPublishEnabled_PublishingFeatureDisabled_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				ProfileFeatures = new ProfileFeatures
				{
					Publishing = new Publishing { Enabled = true }
				},
				PublishTargets = new PublishTargets
				{
					Ftp = new FtpTarget { Enabled = true }
				}
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test",
					[
						new AppSettingsPublishProfiles
						{
							ProfileFeatures = new ProfileFeatures
							{
								Publishing = new Publishing { Enabled = false }
							},
							PublishTargets = new PublishTargets
							{
								Ftp = new FtpTarget { Enabled = true }
							}
						}
					]
				}
			}
		};

		var result = new PublishPreflight(appSettings,
			new ConsoleWrapper(), new FakeSelectorStorage(), new FakeIWebLogger())
			.IsFtpPublishEnabled("test");

		Assert.IsFalse(result);
	}
}
