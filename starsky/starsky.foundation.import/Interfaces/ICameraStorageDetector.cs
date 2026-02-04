using System.Collections.Generic;

namespace starsky.foundation.import.Interfaces;

public interface ICameraStorageDetector
{
	IEnumerable<string> FindCameraStorages();
}
