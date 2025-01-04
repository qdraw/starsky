using System.Threading.Tasks;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIVideoProcess : IVideoProcess
{
	public Task<VideoResult> RunVideo(string subPath, string? beforeFileHash,
		VideoProcessTypes type)
	{
		return Task.FromResult(new VideoResult(true));
	}
}
