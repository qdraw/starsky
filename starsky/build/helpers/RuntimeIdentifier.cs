using System.Runtime.InteropServices;
using Constants;
using Serilog;

namespace helpers;

public static class RuntimeIdentifier
{
	public static string GetCurrentRuntimeIdentifier()
	{
		var os = string.Empty;
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			os = "win";
		}

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
			 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) )
		{
			os = "linux";
		}

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			os = "osx";
		}

		var architecture = RuntimeInformation.OSArchitecture.ToString().ToLower();

		return $"{os}-{architecture}";
	}


	public static bool IsReadyToRunSupported(string toRuntimeIdentifier)
	{
		var currentIdentifier = GetCurrentRuntimeIdentifier();
		return IsReadyToRunSupported(currentIdentifier, toRuntimeIdentifier);
	}

	static bool IsReadyToRunSupported(string currentIdentifier, string toRuntimeIdentifier)
	{
		if ( ReadyToRunSupportedPlatforms.SupportedPlatforms.TryGetValue(currentIdentifier,
				out var supportedTargets) )
		{
			return supportedTargets.Contains(toRuntimeIdentifier);
		}

		// Handle unsupported currentIdentifier
		Log.Error("Unsupported currentIdentifier: {CurrentIdentifier}", currentIdentifier);
		return false;
	}
}
