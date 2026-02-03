using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public class PublishPreflightPublishableTests
{
	private static PublishPreflight CreatePublishPreflight(AppSettings appSettings)
	{
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var console = new ConsoleWrapper();
		var logger = new FakeIWebLogger();

		return new PublishPreflight(appSettings, console, selectorStorage, logger);
	}

	[TestMethod]
	public void IsProfilePublishable_WithWebPublishEnabled_ReturnsTrue()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "test.cshtml",
							WebPublish = true
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("test_profile");

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsProfilePublishable_WithWebPublishDisabled_ReturnsFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "test.cshtml",
							WebPublish = false
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("test_profile");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsProfilePublishable_WithMultipleItemsAllEnabled_ReturnsTrue()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "test.cshtml",
							WebPublish = true
						},
						new() { ContentType = TemplateContentType.Jpeg, WebPublish = true },
						new()
						{
							ContentType = TemplateContentType.PublishContent,
							WebPublish = true
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("test_profile");

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsProfilePublishable_WithMultipleItemsOneDisabled_ReturnsFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "test.cshtml",
							WebPublish = true
						},
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							WebPublish = false // One is disabled
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("test_profile");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsProfilePublishable_ProfileNotFound_ReturnsFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>()
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("nonexistent_profile");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsProfilePublishable_EmptyProfileName_ReturnsFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = true } }
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable(string.Empty);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsProfilePublishable_NullProfileName_ReturnsFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test_profile",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = true } }
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable(null!);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsProfilePublishable_DefaultProfile_ReturnsTrue()
	{
		// Arrange - simulate default profile being publishable
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"_default",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "default.cshtml",
							WebPublish = true
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var result = preflight.IsProfilePublishable("_default");

		// Assert
		Assert.IsTrue(result);
	}
}
