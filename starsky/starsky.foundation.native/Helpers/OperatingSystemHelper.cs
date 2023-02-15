using System.Runtime.InteropServices;

namespace starsky.foundation.native.Helpers;

public static class OperatingSystemHelper
{
	public static OSPlatform GetPlatform() =>
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
		RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
		RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
		RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? OSPlatform.FreeBSD :
		OSPlatform.Create("Unknown");
}
