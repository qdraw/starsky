using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public class WebHtmlPublishServiceOptimizersTests
{
	[TestMethod]
	public async Task GenerateJpeg_UsesProfileOptimizers_WhenDefined()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				Optimizers =
				[
					new Optimizer
					{
						Id = "defaults",
						Enabled = true,
						ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg]
					}
				]
			}
		};
		var profile = new AppSettingsPublishProfiles
		{
			ContentType = TemplateContentType.Jpeg,
			MetaData = false,
			SourceMaxWidth = 1001,
			Optimizers =
			[
				new Optimizer
				{
					Id = "mozjpeg",
					Enabled = true,
					ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
					Options = new OptimizerOptions { Quality = 75 }
				}
			]
		};

		var storage = new FakeIStorage([], ["/test.jpg"], [CreateAnImage.Bytes.ToArray()]);
		var selectorStorage = new FakeSelectorStorage(storage);
		var fakeOptimisationService = new FakeImageOptimisationService();

		var sut = new WebHtmlPublishService(new FakeIPublishPreflight([profile]),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage),
			fakeOptimisationService);

		await sut.GenerateJpeg(profile,
			[new FileIndexItem("/test.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.jpg }],
			Path.DirectorySeparatorChar.ToString(), 1);

		Assert.IsTrue(fakeOptimisationService.Called);
		Assert.IsNotNull(fakeOptimisationService.ReceivedOptimizers);
		Assert.HasCount(1, fakeOptimisationService.ReceivedOptimizers);
		Assert.AreEqual("mozjpeg", fakeOptimisationService.ReceivedOptimizers[0].Id);
	}

	[TestMethod]
	public async Task GenerateJpeg_UsesDefaultOptimizers_WhenProfileHasNone()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				Optimizers =
				[
					new Optimizer
					{
						Id = "mozjpeg",
						Enabled = true,
						ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
						Options = new OptimizerOptions { Quality = 80 }
					}
				]
			}
		};
		var profile = new AppSettingsPublishProfiles
		{
			ContentType = TemplateContentType.Jpeg,
			MetaData = false,
			SourceMaxWidth = 1001
		};

		var storage = new FakeIStorage([], ["/test.jpg"], [CreateAnImage.Bytes.ToArray()]);
		var selectorStorage = new FakeSelectorStorage(storage);
		var fakeOptimisationService = new FakeImageOptimisationService();

		var sut = new WebHtmlPublishService(new FakeIPublishPreflight([profile]),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage),
			fakeOptimisationService);

		await sut.GenerateJpeg(profile,
			[new FileIndexItem("/test.jpg") { ImageFormat = ExtensionRolesHelper.ImageFormat.jpg }],
			Path.DirectorySeparatorChar.ToString(), 1);

		Assert.IsTrue(fakeOptimisationService.Called);
		Assert.IsNotNull(fakeOptimisationService.ReceivedOptimizers);
		Assert.HasCount(1, fakeOptimisationService.ReceivedOptimizers);
		Assert.AreEqual("mozjpeg", fakeOptimisationService.ReceivedOptimizers[0].Id);
	}


}
