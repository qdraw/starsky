using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace starsky.foundation.platform.Helpers;

public static class PlatformParser
{
	public static OSPlatform? GetCurrentOsPlatform()
	{
		OSPlatform? currentPlatform = null;
		foreach ( var platform in new List<OSPlatform>
		         {
			         OSPlatform.Linux,
			         OSPlatform.Windows,
			         OSPlatform.OSX,
			         OSPlatform.FreeBSD
		         }.Where(RuntimeInformation.IsOSPlatform) )
		{
			currentPlatform = platform;
		}

		return currentPlatform;
	}
	
	public static string GetCurrentArchitecture()
	{
		return RuntimeInformation.RuntimeIdentifier;
	}
}
