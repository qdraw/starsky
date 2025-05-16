using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies.Interfaces;

public interface IFfMpegDownloadBinaries
{
	Task<FfmpegDownloadStatus> Download(
		KeyValuePair<BinaryIndex?, List<Uri>> binaryIndexKeyValuePair, string currentArchitecture,
		int retryInSeconds = 15);
}
