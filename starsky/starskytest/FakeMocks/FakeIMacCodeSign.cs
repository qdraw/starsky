using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMacCodeSign : IMacCodeSign
{
	private readonly Dictionary<string, bool> _expectedResults;

	public FakeIMacCodeSign(Dictionary<string, bool>? expectedResults = null)
	{
		_expectedResults = expectedResults ?? new Dictionary<string, bool>();
	}

	public Task<bool> MacCodeSignAndXattrExecutable(string exeFile)
	{
		_expectedResults.TryGetValue(exeFile, out var result);
		return Task.FromResult(result);
	}
}
