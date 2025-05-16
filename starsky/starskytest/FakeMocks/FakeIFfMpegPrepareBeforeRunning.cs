using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFfMpegPrepareBeforeRunning(bool isReady = true) : IFfMpegPrepareBeforeRunning
{
	public Task<bool> PrepareBeforeRunning(string currentArchitecture)
	{
		return Task.FromResult(isReady);
	}
}
