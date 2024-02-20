using System.Runtime.InteropServices;

namespace starsky.foundation.native.Helpers;

public static class OperatingSystemHelper
{
	public static OSPlatform GetPlatform()
	{
		return GetPlatformInternal(RuntimeInformation.IsOSPlatform);
	}

	internal delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);

	/// <summary>
	/// Used to make the function testable
	/// </summary>
	/// <param name="isOsPlatformDelegate">Delegate to know what the OS is</param>
	/// <returns>Runtime OS</returns>
	internal static OSPlatform GetPlatformInternal(IsOsPlatformDelegate isOsPlatformDelegate)
	{
		if ( isOsPlatformDelegate(OSPlatform.Windows) )
		{
			return OSPlatform.Windows;
		}

		if ( isOsPlatformDelegate(OSPlatform.OSX) )
		{
			return OSPlatform.OSX;
		}

		if ( isOsPlatformDelegate(OSPlatform.Linux) )
		{
			return OSPlatform.Linux;
		}

		return isOsPlatformDelegate(OSPlatform.FreeBSD)
			? OSPlatform.FreeBSD
			: OSPlatform.Create("Unknown");
	}
}
