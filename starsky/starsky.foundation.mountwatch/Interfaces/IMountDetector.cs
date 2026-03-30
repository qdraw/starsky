using System.Collections.Generic;

namespace starsky.foundation.mountwatch.Interfaces;

/// <summary>
///     Abstraction for detecting camera storage on mounted volumes
/// </summary>
public interface IMountDetector
{
	/// <summary>
	///     Check if a mount path contains camera storage (e.g., DCIM)
	/// </summary>
	/// <param name="mountPath">Full path to mount point</param>
	/// <returns>True if camera storage is detected</returns>
	bool HasCameraStorage(string mountPath);

	/// <summary>
	///     Get all camera storage paths on a mount
	/// </summary>
	/// <param name="mountPath">Full path to mount point</param>
	/// <returns>List of camera storage paths</returns>
	IEnumerable<string> GetCameraStoragePaths(string mountPath);
}

