namespace starsky.foundation.video.Process.Interfaces;

public interface IVideoProcessThumbnailPost
{
	Task<VideoResult> PostPrepThumbnail(VideoResult runResult,
		Stream stream,
		string subPath);
}
