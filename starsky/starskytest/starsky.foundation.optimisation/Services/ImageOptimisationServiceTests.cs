using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public class ImageOptimisationServiceTests
{
	[TestMethod]
	public async Task Optimize_WithNoImages_DoesNotDownload()
	{
		var appSettings = new AppSettings();
		var fakeDownloaders = new FakeMozJpegDownload();
		var fakeMozJpeg = new MozJpegService(appSettings,
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeDownloaders);

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

		Assert.AreEqual(0, fakeDownloaders.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_UsesDefaults_WhenOptimizersNull_AndMatchesImageFormat()
	{
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
		var fakeDownloaders = new FakeMozJpegDownload();
		var fakeMozJpeg = new MozJpegService(appSettings,
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeDownloaders);

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
		]);

		Assert.AreEqual(1, fakeDownloaders.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_SkipsWhenFormatDoesNotMatch()
	{
		var fakeDownloaders = new FakeMozJpegDownload();
		var fakeMozJpeg = new MozJpegService(new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeDownloaders);

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

		Assert.AreEqual(0, fakeDownloaders.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_SkipsWhenOptimizerDisabled()
	{
		var fakeDownloaders = new FakeMozJpegDownload();
		var fakeMozJpeg = new MozJpegService(new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeDownloaders);

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

		Assert.AreEqual(0, fakeDownloaders.DownloadCount);
	}

	[TestMethod]
	public async Task Optimize_NonJpegOutput_SkipsOptimizerCommand()
	{
		var fakeDownloaders = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
		var appSettings = new AppSettings { DependenciesFolder = "/dependencies" };
		var architecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var exeName = appSettings.IsWindows ? "cjpeg.exe" : "cjpeg";
		var cjpegPath = $"/dependencies/mozjpeg/{architecture}/{exeName}";

		var fakeMozJpeg = new MozJpegService(new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger(), fakeDownloaders);

		var storage = new FakeIStorage([], ["/out.jpg", cjpegPath],
			[[.. CreateAnPng.Bytes], [1]]);
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

		Assert.AreEqual(1, fakeDownloaders.DownloadCount);
		Assert.IsTrue(storage.ExistFile("/out.jpg"));
		Assert.IsFalse(storage.ExistFile("/out.jpg.optimizing"));
	}

	private sealed class FakeMozJpegDownload : IMozJpegDownload
	{
		private readonly ImageOptimisationDownloadStatus _status;

		public FakeMozJpegDownload(ImageOptimisationDownloadStatus status =
			ImageOptimisationDownloadStatus.DownloadBinariesFailed)
		{
			_status = status;
		}

		public int DownloadCount { get; private set; }

		public Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
			int retryInSeconds = 15)
		{
			DownloadCount++;
			return Task.FromResult(_status);
		}
	}
}
