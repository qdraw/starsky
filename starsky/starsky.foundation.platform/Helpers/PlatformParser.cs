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
			         OSPlatform.Linux, OSPlatform.Windows, OSPlatform.OSX, OSPlatform.FreeBSD
		         }.Where(RuntimeInformation.IsOSPlatform) )
		{
			currentPlatform = platform;
		}

		return currentPlatform;
	}

	public static OSPlatform? RuntimeIdentifier(string? runtimeIdentifier)
	{
		if ( runtimeIdentifier == null )
		{
			return null;
		}

		if ( runtimeIdentifier.StartsWith("win-") )
		{
			return OSPlatform.Windows;
		}

		if ( runtimeIdentifier.StartsWith("linux-") )
		{
			return OSPlatform.Linux;
		}

		return runtimeIdentifier.StartsWith("osx-") ? OSPlatform.OSX : null;
	}
}
