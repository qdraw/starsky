using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public class PublishPreflightIntegrationTests
{
	private static PublishPreflight CreatePublishPreflight(AppSettings appSettings)
	{
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var logger = new FakeIWebLogger();
		var console = new FakeConsoleWrapper();

		return new PublishPreflight(appSettings, console, selectorStorage, logger);
	}

	[TestMethod]
	public void PublishProfileWorkflow_DefaultProfilePublishable()
	{
		// Scenario: User has a default profile configured to be publishable
		// Expected: Can publish with _default profile

		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"_default", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "default.cshtml",
							WebPublish = true // Explicitly enabled
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var isValid = preflight.IsProfileValid("_default").Item1;
		var isPublishable = preflight.IsProfilePublishable("_default");
		var allProfiles = preflight.GetAllPublishProfileNames();

		// Assert
		Assert.IsTrue(isValid, "Profile should be valid");
		Assert.IsTrue(isPublishable, "Profile should be publishable");
		Assert.IsNotNull(allProfiles);
	}

	[TestMethod]
	public void PublishProfileWorkflow_StagingProfileNotPublishable()
	{
		// Scenario: Staging profile exists for testing but should not be publishable
		// Expected: Can export with staging profile but not publish to FTP

		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"staging", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "staging.cshtml",
							WebPublish = false // Explicitly disabled for safety
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var isValid = preflight.IsProfileValid("staging").Item1;
		var isPublishable = preflight.IsProfilePublishable("staging");

		// Assert
		Assert.IsTrue(isValid, "Profile should be valid for export");
		Assert.IsFalse(isPublishable, "Profile should not be publishable");
	}

	[TestMethod]
	public void PublishProfileWorkflow_MultipleProfilesMixedPublishability()
	{
		// Scenario: Multiple profiles, some publishable, some not
		// Expected: GetAllPublishableProfiles filters correctly

		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"production",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "prod.cshtml",
							WebPublish = true
						},
						new() { ContentType = TemplateContentType.Jpeg, WebPublish = true }
					}
				},
				{
					"development",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "dev.cshtml",
							WebPublish = false
						}
					}
				},
				{
					"testing",
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
		var productionPublishable = preflight.IsProfilePublishable("production");
		var developmentPublishable = preflight.IsProfilePublishable("development");
		var testingPublishable = preflight.IsProfilePublishable("testing");

		// Assert
		Assert.IsTrue(productionPublishable, "Production should be publishable");
		Assert.IsFalse(developmentPublishable, "Development should not be publishable");
		Assert.IsTrue(testingPublishable, "Testing should be publishable");
	}

	[TestMethod]
	public void PublishProfileWorkflow_PartialProfileNotPublishable()
	{
		// Scenario: Profile with multiple items, one not publishable
		// Expected: Entire profile is not publishable (all-or-nothing safety)

		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"mixed", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "index.cshtml",
							WebPublish = true
						},
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							WebPublish =
								false // Even one false makes entire profile non-publishable
						},
						new()
							{
								ContentType = TemplateContentType.PublishContent, WebPublish = true
							}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var isPublishable = preflight.IsProfilePublishable("mixed");

		// Assert
		Assert.IsFalse(isPublishable,
			"Profile with any non-publishable item should not be publishable");
	}

	[TestMethod]
	public void PublishProfileWorkflow_ProfileValidationIndependentOfPublishability()
	{
		// Scenario: Profile can be valid (correct structure) but not publishable (WebPublish=false)
		// Expected: Validation and publishability are independent checks

		// Arrange
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"export_only", new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Template = "export.cshtml",
							Path = "index.html",
							WebPublish = false // Not publishable
						}
					}
				}
			}
		};

		var preflight = CreatePublishPreflight(appSettings);

		// Act
		var validationResult = preflight.IsProfileValid("export_only");
		var isValid = validationResult.Item1;
		var errors = validationResult.Item2;
		var isPublishable = preflight.IsProfilePublishable("export_only");

		// Assert
		Assert.IsTrue(isValid, "Profile should be structurally valid");
		Assert.AreEqual(0, errors.Count, "Profile should have no validation errors");
		Assert.IsFalse(isPublishable, "Profile should not be publishable due to WebPublish=false");
	}
}
