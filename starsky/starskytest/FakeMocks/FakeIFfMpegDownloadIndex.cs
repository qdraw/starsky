using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starskytest.FakeMocks;

public class FakeIFfMpegDownloadIndex : IFfMpegDownloadIndex
{
	private readonly FfmpegBinariesContainer _ffmpegBinariesContainer;

	public FakeIFfMpegDownloadIndex(FfmpegBinariesContainer? ffmpegBinariesContainer = null)
	{
		_ffmpegBinariesContainer = ffmpegBinariesContainer ??
		                           new FfmpegBinariesContainer(string.Empty,
			                           null, [], false);
	}

	public Task<FfmpegBinariesContainer> DownloadIndex()
	{
		return Task.FromResult(_ffmpegBinariesContainer);
	}
}
