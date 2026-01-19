using System.Collections.Generic;
using starsky.foundation.import.Interfaces;

namespace starskytest.FakeMocks
{
    public class FakeCameraStorageDetector(List<string> paths) : ICameraStorageDetector
    {
	    public IEnumerable<string> FindCameraStorages()
        {
            return paths;
        }
    }
}

