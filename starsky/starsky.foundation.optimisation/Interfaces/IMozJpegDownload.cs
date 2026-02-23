using starsky.foundation.optimisation.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IMozJpegDownload
{
	Task<ImageOptimisationDownloadStatus> Download(string? architecture = null, int retryInSeconds = 15);
}
