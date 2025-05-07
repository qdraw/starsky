using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starskytest.FakeMocks;

public class FakeIFfMpegDownloadIndex(FfmpegBinariesContainer? ffmpegBinariesContainer = null)
	: IFfMpegDownloadIndex
{
	private readonly FfmpegBinariesContainer _ffmpegBinariesContainer = ffmpegBinariesContainer ??
	                                                                    new FfmpegBinariesContainer(string.Empty,
		                                                                    null, [], false);

	public int Count { get; set; }

	public Task<FfmpegBinariesContainer> DownloadIndex()
	{
		Count++;
		return Task.FromResult(_ffmpegBinariesContainer);
	}
}
