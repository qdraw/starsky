using starsky.foundation.optimisation.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IImageOptimisationToolDownload
{
	/// <summary>
	///     Downloads the image optimisation tool for multiple architectures. It iterates through the
	///     provided architectures and calls the Download method for each architecture. The results are
	///     collected in a list of ImageOptimisationDownloadStatus and returned at the end.
	/// </summary>
	/// <param name="options">which tool to download</param>
	/// <param name="architectures">use the .net names for architectures</param>
	/// <returns>status of download</returns>
	Task<List<ImageOptimisationDownloadStatus>> Download(
		ImageOptimisationToolDownloadOptions options,
		List<string> architectures);

	/// <summary>
	///     Downloads the image optimisation tool for a specific architecture. It checks if the tool is
	///     already downloaded and if not, it downloads the zip file, checks the sha256 hash, extracts the
	///     zip
	/// </summary>
	/// <param name="options">which tool to download</param>
	/// <param name="architecture">use .net names for architecture</param>
	/// <param name="retryInSeconds">to retry</param>
	/// <returns>status of download</returns>
	Task<ImageOptimisationDownloadStatus> Download(ImageOptimisationToolDownloadOptions options,
		string? architecture = null, int retryInSeconds = 15);
}
