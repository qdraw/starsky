using System.IO;
using System.Threading.Tasks;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIVideoProcessThumbnailPost : IVideoProcessThumbnailPost
{
	public Task<VideoResult> PostPrepThumbnail(VideoResult runResult,
		Stream stream, string subPath, string? beforeFileHash)
	{
		return Task.FromResult(new VideoResult(true, subPath,
			SelectorStorage.StorageServices.Temporary, "Mocked"));
	}
}
