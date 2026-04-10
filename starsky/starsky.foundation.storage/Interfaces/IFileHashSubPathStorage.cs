using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.storage.Interfaces;

public interface IFileHashSubPathStorage
{
	Task<KeyValuePair<string, bool>> GetHashCodeAsync(
		string fullFileName,
		ExtensionRolesHelper.ImageFormat? imageFormat,
		int timeoutInMilliseconds = 30000);
}
