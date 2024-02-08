using System.Collections.Generic;
using System.Runtime.InteropServices;
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

	/// <summary>
	/// SDK-platform 	Supported target platforms
	/// 	Windows X64 	Windows (X86, X64, Arm64), Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	/// win-x64 - supports: win-x86, win-x64, win-arm64, linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	/// Windows X86 	Windows (X86), Linux (Arm32)
	/// win-x86 - supports: win-x86, linux-arm
	/// Linux X64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	/// linux-x64 - supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	/// Linux Arm32 	Linux Arm32
	/// linux-arm, supports linux-arm
	/// Linux Arm64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	/// linux-arm64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64 
	/// macOS X64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	/// osx-x64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64 
	/// macOS Arm64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	/// osx-arm64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	/// 
	/// @see: https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run
	/// </summary>
	static readonly Dictionary<string, List<string>> SupportedPlatforms = new()
	{
		{ "win-x64",
			[
				"win-x86", "win-x64", "win-arm64", "linux-x64", "linux-arm",
				"linux-arm64", "osx-x64", "osx-arm64"
			]
		},
		{ "win-x86", ["win-x86", "linux-arm"] },
		{ "linux-x64",
			["linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64"]
		},
		{ "linux-arm", ["linux-arm"] },
		{ "linux-arm64",
			["linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64"]
		},
		{ "osx-x64",
			["linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64"]
		},
		{ "osx-arm64",
			["linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64"]
		}
	};

	public static bool IsReadyToRunSupported(string toRuntimeIdentifier)
	{
		var currentIdentifier = GetCurrentRuntimeIdentifier();
		return IsReadyToRunSupported(currentIdentifier, toRuntimeIdentifier);
	}

	static bool IsReadyToRunSupported(string currentIdentifier, string toRuntimeIdentifier)
	{
		if ( SupportedPlatforms.TryGetValue(currentIdentifier, out var supportedTargets) )
		{
			return supportedTargets.Contains(toRuntimeIdentifier);
		}

		// Handle unsupported currentIdentifier
		Log.Error($"Unsupported currentIdentifier: {currentIdentifier}");
		return false;
	}
}
