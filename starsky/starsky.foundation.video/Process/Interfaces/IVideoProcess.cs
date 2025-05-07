using starsky.foundation.storage.Storage;

namespace starsky.foundation.video.Process.Interfaces;

public interface IVideoProcess
{
	Task<VideoResult> RunVideo(string subPath,
		string beforeFileHash, VideoProcessTypes type);

	bool CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType);
}
