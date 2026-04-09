using System;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.import.Helpers;

public static class LinuxFileSystemHelper
{
	private const string ProcMountsPath = "/proc/mounts";

	/// <summary>
	///     Attempt to detect the filesystem type (for Linux)
	/// </summary>
	internal static string DetectFileSystem(IStorage hostStorage, string mountPath)
	{
		try
		{
			// Try to read from /proc/mounts if on Linux
			if ( hostStorage.ExistFile(ProcMountsPath) )
			{
				var lines = hostStorage.ReadAllLines(ProcMountsPath);
				foreach ( var line in lines )
				{
					var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
					if ( parts.Length >= 3 && parts[1] == mountPath )
					{
						return parts[2]; // Third field is filesystem type
					}
				}
			}
		}
		catch
		{
			// Ignore errors
		}

		return string.Empty;
	}
}
