using starsky.foundation.optimisation.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IImageOptimisationToolDownload
{
	Task<ImageOptimisationDownloadStatus> Download(ImageOptimisationToolDownloadOptions options,
		string? architecture = null, int retryInSeconds = 15);
}
