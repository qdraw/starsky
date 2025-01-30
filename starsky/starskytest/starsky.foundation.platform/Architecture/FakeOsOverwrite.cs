using System.Runtime.InteropServices;

namespace starskytest.starsky.foundation.platform.Architecture;

internal static class FakeOsOverwrite
{
	internal static bool IsWindows(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Windows;
	}

	internal static bool IsMacOs(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.OSX;
	}

	internal static bool IsLinux(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Linux;
	}

	internal static bool IsFreeBsd(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.FreeBSD;
	}

	internal static bool IsUnknown(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Create("Unknown");
	}
}
