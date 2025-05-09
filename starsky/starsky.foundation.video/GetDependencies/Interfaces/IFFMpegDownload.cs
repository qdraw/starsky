using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegDownload
{
	Task<List<FfmpegDownloadStatus>> DownloadFfMpeg(List<string> architectures);
	Task<FfmpegDownloadStatus> DownloadFfMpeg(string? architecture = null);
	string GetSetFfMpegPath();
}
