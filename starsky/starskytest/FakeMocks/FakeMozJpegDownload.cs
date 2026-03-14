using System.Threading.Tasks;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;

namespace starskytest.FakeMocks;

public sealed class FakeMozJpegDownload(
	ImageOptimisationDownloadStatus status =
		ImageOptimisationDownloadStatus.DownloadBinariesFailed)
	: IMozJpegDownload
{
	public delegate Task<bool> FixPermissionsDelegateHandler(string exePath);

	public int DownloadCount { get; private set; }

	public FixPermissionsDelegateHandler? FixPermissionsDelegate { get; set; }

	public Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
		int retryInSeconds = 15)
	{
		DownloadCount++;
		return Task.FromResult(status);
	}

	/// <summary>
	///     Add delegate
	/// </summary>
	/// <param name="exePath"></param>
	/// <returns></returns>
	public async Task<bool> FixPermissions(string exePath)
	{
		if ( FixPermissionsDelegate == null )
		{
			return true;
		}

		return await FixPermissionsDelegate(exePath);
	}
}
