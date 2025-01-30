using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.Services;

[Service(typeof(IFileHashSubPathStorage), InjectionLifetime = InjectionLifetime.Scoped)]
public class FileHashSubPathStorage(ISelectorStorage selectorStorage, IWebLogger logger)
	: IFileHashSubPathStorage
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public async Task<KeyValuePair<string, bool>> GetHashCodeAsync(string fullFileName,
		int timeoutInMilliseconds = 30000)
	{
		return await new FileHash(_storage, logger).GetHashCodeAsync(fullFileName,
			timeoutInMilliseconds);
	}
}
