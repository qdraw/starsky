using System.Threading.Tasks;
using System.Linq;
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

	[TestMethod]
	public async Task Optimize_NonJpegOutput_SkipsOptimizerCommand()
	{
		var appSettings = new AppSettings
		{
			DependenciesFolder = "/dependencies"
		};
		var architecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var exeName = appSettings.IsWindows ? "cjpeg.exe" : "cjpeg";
		var cjpegPath = $"/dependencies/mozjpeg/{architecture}/{exeName}";

		var storage = new FakeIStorage([], ["/out.jpg", cjpegPath],
			[CreateAnPng.Bytes.ToArray(), [1]]);
		var fakeMozJpeg = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
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

		Assert.AreEqual(1, fakeMozJpeg.DownloadCount);
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