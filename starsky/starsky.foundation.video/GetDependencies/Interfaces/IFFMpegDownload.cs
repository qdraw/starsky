using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegDownload
{
	Task<FfmpegDownloadStatus> DownloadFfMpeg();

	string GetSetFfMpegPath();
}
