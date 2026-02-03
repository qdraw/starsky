using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.Controllers;

[TestClass]
public class PublishControllerPublishableTests
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
	public void PublishProfileFiltering_WithPublishableAndNonPublishable_FiltersCorrectly()
	{
		// Arrange - Simulate what GetPublishableProfiles() does
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"publishable_profile",
					new List<AppSettingsPublishProfiles>
					{
						new() { ContentType = TemplateContentType.Html, WebPublish = true }
					}
				},
				{
					"not_publishable_profile",
					new List<AppSettingsPublishProfiles>
					{
						new() { ContentType = TemplateContentType.Html, WebPublish = false }
					}
				},
				{
					"export_only_profile",
					new List<AppSettingsPublishProfiles>
					{
						new() { ContentType = TemplateContentType.Html, WebPublish = false }
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act - Simulate filtering logic
		var allProfiles = preflight.GetAllPublishProfileNames();
		var publishableCount = 0;
		foreach ( var profile in allProfiles )
		{
			if ( preflight.IsProfilePublishable(profile.Key) )
			{
				publishableCount++;
			}
		}

		// Assert
		Assert.AreEqual(1, publishableCount, "Only one profile should be publishable");
	}

	[TestMethod]
	public void PublishProfileValidation_NonPublishableProfile_IsDetected()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"not_publishable",
					new List<AppSettingsPublishProfiles>
					{
						new() { ContentType = TemplateContentType.Html, WebPublish = false }
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var isPublishable = preflight.IsProfilePublishable("not_publishable");

		// Assert
		Assert.IsFalse(isPublishable);
	}

	[TestMethod]
	public void PublishProfileValidation_PublishableProfile_IsAccepted()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"publishable",
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
		var isPublishable = preflight.IsProfilePublishable("publishable");

		// Assert
		Assert.IsTrue(isPublishable);
	}

	[TestMethod]
	public void PublishProfileFiltering_AllDisabled_ReturnsEmpty()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"disabled1",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = false } }
				},
				{
					"disabled2",
					new List<AppSettingsPublishProfiles> { new() { WebPublish = false } }
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var allProfiles = preflight.GetAllPublishProfileNames();
		var publishableCount = 0;
		foreach ( var profile in allProfiles )
		{
			if ( preflight.IsProfilePublishable(profile.Key) )
			{
				publishableCount++;
			}
		}

		// Assert
		Assert.AreEqual(0, publishableCount);
	}
}
