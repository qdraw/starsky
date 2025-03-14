using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMacCodeSign : IMacCodeSign
{
	private readonly Dictionary<string, bool?> _expectedResults = new();
	private readonly IStorage? _storage;

	public FakeIMacCodeSign(Dictionary<string, bool?>? expectedResults = null)
	{
		_expectedResults = expectedResults ?? new Dictionary<string, bool?>();
	}

	public FakeIMacCodeSign(IStorage storage)
	{
		_storage = storage;
	}

	public Task<bool?> MacCodeSignAndXattrExecutable(string exeFile)
	{
		if ( _storage?.ExistFile(
			    new MacCodeSign(new FakeSelectorStorage(_storage), new FakeIWebLogger())
				    .CodeSignPath) == true )
		{
			return Task.FromResult(( bool? ) true);
		}

		_expectedResults.TryGetValue(exeFile, out var result);
		return Task.FromResult(result);
	}
}
