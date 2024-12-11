using System.Runtime.InteropServices;

namespace starsky.foundation.platform.Architecture;

public static class CurrentArchitecture
{
	public delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);

	public delegate System.Runtime.InteropServices.Architecture OsArchitectureDelegate();

	public static string GetCurrentRuntimeIdentifier()
	{
		return GetCurrentRuntimeIdentifier(RuntimeInformation.IsOSPlatform,
			GetOsArchitecture);
	}

	public static string GetCurrentRuntimeIdentifier(IsOsPlatformDelegate isOsPlatformDelegate,
		OsArchitectureDelegate architectureDelegate)
	{
		var os = string.Empty;
		if ( isOsPlatformDelegate(OSPlatform.Windows) )
		{
			os = "win";
		}

		if ( isOsPlatformDelegate(OSPlatform.Linux) || isOsPlatformDelegate(OSPlatform.FreeBSD) )
		{
			os = "linux";
		}

		if ( isOsPlatformDelegate(OSPlatform.OSX) )
		{
			os = "osx";
		}

		var architecture = architectureDelegate().ToString().ToLowerInvariant();

		return $"{os}-{architecture}";
	}

	private static System.Runtime.InteropServices.Architecture GetOsArchitecture()
	{
		return RuntimeInformation.OSArchitecture;
	}
}
