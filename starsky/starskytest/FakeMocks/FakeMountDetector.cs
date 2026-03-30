using System.Collections.Generic;
using System.Linq;
using starsky.foundation.mountwatch.Interfaces;

namespace starskytest.FakeMocks;

public class FakeMountDetector : IMountDetector
{
	public bool HasCameraStorage(string mountPath)
	{
		return false;
	}

	public IEnumerable<string> GetCameraStoragePaths(string mountPath)
	{
		return Enumerable.Empty<string>();
	}
}
