using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFileHashSubPathStorage(List<(string, string, bool)> storage)
	: IFileHashSubPathStorage
{
	/// <summary>
	/// 1. FullFileName
	/// 2. FileHash
	/// 3. IsSuccess
	/// </summary>
	private readonly List<(string, string, bool)> _storage = storage;

	public Task<KeyValuePair<string, bool>> GetHashCodeAsync(string fullFileName, int timeoutInMilliseconds = 30000)
	{
		var result = _storage.FirstOrDefault(p => p.Item1 == fullFileName);
		return Task.FromResult(new KeyValuePair<string, bool>(result.Item2, result.Item3));
	}
}
