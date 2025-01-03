namespace starsky.foundation.video.Process.Interfaces;

public interface IVideoProcess
{
	Task<bool> Run(string subPath,
		string? beforeFileHash, VideoProcessTypes type);
}
