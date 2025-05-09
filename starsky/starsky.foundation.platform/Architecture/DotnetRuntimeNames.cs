using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.platform.Architecture;

public static class DotnetRuntimeNames
{
	internal const string GenericRuntimeName = "generic-netcore";

	internal const string OsWindowsPrefix = "win";
	internal const string OsLinuxPrefix = "linux";
	internal const string OsMacOsPrefix = "osx";

	public static bool IsWindows(string architecture)
	{
		return architecture.StartsWith(OsWindowsPrefix);
	}

	public static IEnumerable<string> GetArchitecturesNoGenericAndFallback(
		List<string> architectures)
	{
		if ( architectures.Count == 0 )
		{
			architectures.Add(CurrentArchitecture.GetCurrentRuntimeIdentifier());
		}

		return architectures.Where(p => p !=
		                                GenericRuntimeName);
	}
}
