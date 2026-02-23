using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public class ImageOptimisationServiceTests
{
	[TestMethod]
	public async Task Optimize_WithNoImages_DoesNotDownload()
	{
		var fakeMozJpeg = new FakeMozJpegDownload();
		var appSettings = new AppSettings();
		var sut = new ImageOptimisationService(appSettings,
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeMozJpeg);

		await sut.Optimize([],
			[
				new Optimizer
				{
					Enabled = true,
					Id = "mozjpeg",
					ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
					Options = new OptimizerOptions { Quality = 80 }
				}
			]);

		Assert.AreEqual(0, fakeMozJpeg.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_UsesDefaults_WhenOptimizersNull_AndMatchesImageFormat()
	{
		var fakeMozJpeg = new FakeMozJpegDownload();
		var appSettings = new AppSettings
		{
			PublishProfilesDefaults = new AppSettingsPublishProfilesDefaults
			{
				Optimizers =
				[
					new Optimizer
					{
						Enabled = true,
						Id = "mozjpeg",
						ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
						Options = new OptimizerOptions { Quality = 80 }
					}
				]
			}
		};

		var storage = new FakeIStorage([], ["/out.jpg"]);
		var sut = new ImageOptimisationService(appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger(), fakeMozJpeg);

		await sut.Optimize(
			[
				new ImageOptimisationItem
				{
					InputPath = "/in.jpg",
					OutputPath = "/out.jpg",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
				}
			], null);

		Assert.AreEqual(1, fakeMozJpeg.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_SkipsWhenFormatDoesNotMatch()
	{
		var fakeMozJpeg = new FakeMozJpegDownload();
		var storage = new FakeIStorage([], ["/out.jpg"]);
		var sut = new ImageOptimisationService(new AppSettings(),
			new FakeSelectorStorage(storage), new FakeIWebLogger(), fakeMozJpeg);

		await sut.Optimize(
			[
				new ImageOptimisationItem
				{
					InputPath = "/in.png",
					OutputPath = "/out.jpg",
					ImageFormat = ExtensionRolesHelper.ImageFormat.png
				}
			],
			[
				new Optimizer
				{
					Enabled = true,
					Id = "mozjpeg",
					ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
					Options = new OptimizerOptions { Quality = 80 }
				}
			]);

		Assert.AreEqual(0, fakeMozJpeg.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_SkipsWhenOptimizerDisabled()
	{
		var fakeMozJpeg = new FakeMozJpegDownload();
		var storage = new FakeIStorage([], ["/out.jpg"]);
		var sut = new ImageOptimisationService(new AppSettings(),
			new FakeSelectorStorage(storage), new FakeIWebLogger(), fakeMozJpeg);

		await sut.Optimize(
			[
				new ImageOptimisationItem
				{
					InputPath = "/in.jpg",
					OutputPath = "/out.jpg",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
				}
			],
			[
				new Optimizer
				{
					Enabled = false,
					Id = "mozjpeg",
					ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
					Options = new OptimizerOptions { Quality = 80 }
				}
			]);

		Assert.AreEqual(0, fakeMozJpeg.DownloadCount);
	}

	private sealed class FakeMozJpegDownload : IMozJpegDownload
	{
		public int DownloadCount { get; private set; }

		public Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
			int retryInSeconds = 15)
		{
			DownloadCount++;
			return Task.FromResult(ImageOptimisationDownloadStatus.DownloadBinariesFailed);
		}
	}
}