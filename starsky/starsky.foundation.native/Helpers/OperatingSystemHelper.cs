using System.Runtime.InteropServices;

namespace starsky.foundation.native.Helpers;

public static class OperatingSystemHelper
{
	public static OSPlatform GetPlatform() {
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			return OSPlatform.Windows;
		}
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			return OSPlatform.OSX;
		}
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) )
		{
			return OSPlatform.Linux;
		}
		return RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? OSPlatform.FreeBSD : OSPlatform.Create("Unknown");
	}
}
