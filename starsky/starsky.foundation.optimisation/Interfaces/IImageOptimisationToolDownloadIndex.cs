using starsky.foundation.optimisation.Models;

namespace starsky.foundation.optimisation.Interfaces;

public interface IImageOptimisationToolDownloadIndex
{
	Task<ImageOptimisationBinariesContainer> DownloadIndex(ImageOptimisationToolDownloadOptions options);
}
