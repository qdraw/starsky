using System;
using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFfMpegPrepareBeforeRunning : IFfMpegPrepareBeforeRunning
{
	public Task<bool> PrepareBeforeRunning(string currentArchitecture)
	{
		throw new NotImplementedException();
	}
}
