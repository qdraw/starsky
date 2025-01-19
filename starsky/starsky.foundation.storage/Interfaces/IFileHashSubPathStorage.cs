using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.storage.Interfaces;

public interface IFileHashSubPathStorage
{
	Task<KeyValuePair<string, bool>> GetHashCodeAsync(
		string fullFileName, int timeoutInMilliseconds = 30000);
}
