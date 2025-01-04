namespace starsky.foundation.video.Process.Interfaces;

public interface IVideoProcess
{
	Task<VideoResult> RunVideo(string subPath,
		string? beforeFileHash, VideoProcessTypes type);
}
