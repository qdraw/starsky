namespace starsky.foundation.platform.Architecture;

public static class DotnetRuntimeNames
{
	public const string GenericRuntimeName = "generic-netcore";

	internal const string OsWindowsPrefix = "win";
	internal const string OsLinuxPrefix = "linux";
	internal const string OsMacOsPrefix = "osx";

	public static bool IsWindows(string architecture)
	{
		return architecture.StartsWith(OsWindowsPrefix);
	}
}
