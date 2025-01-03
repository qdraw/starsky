using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegDownloadIndex
{
	Task<FfmpegBinariesContainer> DownloadIndex();
}
