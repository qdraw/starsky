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

	public static List<(OSPlatform?, Architecture?)> RuntimeIdentifier(string? runtimeIdentifiers)
	{
		var result = new List<(OSPlatform?, Architecture?)>();
		if ( runtimeIdentifiers == null )
		{
			return [];
		}

		var runtimes = runtimeIdentifiers.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();
		foreach ( var runtime in runtimes )
		{
			var singleRuntimeIdentifier = SingleRuntimeIdentifier(runtime);
			if ( singleRuntimeIdentifier is { Item1: not null, Item2: not null } )
			{
				result.Add(singleRuntimeIdentifier);
			}
		}

		return result;
	}

	private static (OSPlatform?, Architecture?) SingleRuntimeIdentifier(string? runtimeIdentifier)
	{
		if ( runtimeIdentifier == null )
		{
			return ( null, null );
		}

		switch ( runtimeIdentifier )
		{
			case "win-x64":
				return ( OSPlatform.Windows, Architecture.X64 );
			case "win-arm64":
				return ( OSPlatform.Windows, Architecture.Arm64 );
			case "linux-arm":
				return ( OSPlatform.Linux, Architecture.Arm );
			case "linux-arm64":
				return ( OSPlatform.Linux, Architecture.Arm64 );
			case "linux-x64":
				return ( OSPlatform.Linux, Architecture.X64 );
			case "osx-x64":
				return ( OSPlatform.OSX, Architecture.X64 );
			case "osx-arm64":
				return ( OSPlatform.OSX, Architecture.Arm64 );
			default:
				return ( null, null );
		}
	}

	public static Architecture GetCurrentArchitecture()
	{
		return Architecture.Wasm;
	}
}
