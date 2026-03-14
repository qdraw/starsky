using System.Threading.Tasks;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;

namespace starskytest.FakeMocks;

public sealed class FakeMozJpegDownload(
	ImageOptimisationDownloadStatus status =
		ImageOptimisationDownloadStatus.DownloadBinariesFailed)
	: IMozJpegDownload
{
	public int DownloadCount { get; private set; }

	public Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
		int retryInSeconds = 15)
	{
		DownloadCount++;
		return Task.FromResult(status);
	}

	public Task<bool> FixPermissions(string exePath)
	{
		return Task.FromResult(true);
	}
}
