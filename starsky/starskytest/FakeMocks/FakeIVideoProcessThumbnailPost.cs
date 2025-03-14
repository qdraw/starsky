using System.IO;
using System.Threading.Tasks;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIVideoProcessThumbnailPost : IVideoProcessThumbnailPost
{
	public Task<VideoResult> PostPrepThumbnail(VideoResult runResult,
		Stream stream, string subPath)
	{
		return Task.FromResult(new VideoResult(true, subPath, "Mocked"));
	}
}
