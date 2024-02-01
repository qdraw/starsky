using System;
using System.Runtime.InteropServices;

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
}
