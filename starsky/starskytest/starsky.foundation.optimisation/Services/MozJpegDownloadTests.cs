using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public class MozJpegDownloadTests
{
	[TestMethod]
	public async Task Download_ForwardsOptionsAndArchitecture_ToGenericDownloader()
	{
		var fake = new FakeImageOptimisationToolDownload();
		var sut = new MozJpegDownload(fake);

		var result = await sut.Download("linux-x64", 9);

		Assert.AreEqual(ImageOptimisationDownloadStatus.Ok, result);
		Assert.IsNotNull(fake.ReceivedOptions);
		Assert.AreEqual("mozjpeg", fake.ReceivedOptions.ToolName);
		Assert.HasCount(2, fake.ReceivedOptions.IndexUrls);
		Assert.AreEqual("https://starsky-dependencies.netlify.app/mozjpeg/index.json",
			fake.ReceivedOptions.IndexUrls[0].ToString());
		Assert.AreEqual("https://qdraw.nl/special/mirror/mozjpeg/index.json",
			fake.ReceivedOptions.IndexUrls[1].ToString());
		Assert.AreEqual("linux-x64", fake.ReceivedArchitecture);
		Assert.AreEqual(9, fake.ReceivedRetrySeconds);
	}

	private sealed class FakeImageOptimisationToolDownload : IImageOptimisationToolDownload
	{
		public ImageOptimisationToolDownloadOptions? ReceivedOptions { get; private set; }
		public string? ReceivedArchitecture { get; private set; }
		public int ReceivedRetrySeconds { get; private set; }

		public Task<ImageOptimisationDownloadStatus> Download(
			ImageOptimisationToolDownloadOptions options, string? architecture = null,
			int retryInSeconds = 15)
		{
			ReceivedOptions = options;
			ReceivedArchitecture = architecture;
			ReceivedRetrySeconds = retryInSeconds;
			return Task.FromResult(ImageOptimisationDownloadStatus.Ok);
		}
	}
}
