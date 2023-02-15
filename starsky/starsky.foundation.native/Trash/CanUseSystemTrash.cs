using System.Runtime.InteropServices;

namespace starsky.foundation.native.Trash;

public static class CanUseSystemTrash
{
	public static bool Detect()
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
		    RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) || 
		    Environment.UserName == "root" || !Environment.UserInteractive )
		{
			return false;
		}
		
		return true;
	}
}
