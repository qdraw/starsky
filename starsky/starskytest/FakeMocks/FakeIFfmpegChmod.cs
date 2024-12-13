using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFfmpegChmod(IStorage hostFileSystemStorage) : IFfmpegChmod
{
	public Task<bool> Chmod(string exeFile)
	{
		return Task.FromResult(hostFileSystemStorage.ExistFile(
			new FfMpegChmod(new FakeSelectorStorage(hostFileSystemStorage), new FakeIWebLogger()).CmdPath));
	}
}
