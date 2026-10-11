using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.settings.Helpers;

internal static class RestrictedPath
{
	// Sensitive system directories that must never be used as mapping targets.
	// Includes both the canonical and common alias forms (e.g. macOS symlinks /etc → /private/etc).
	[SuppressMessage("Sonar",
		"S1075: Refactor your code not to use hardcoded absolute paths or URIs",
		Justification = "Check to not allow this as input for the storage folder mapping")]
	[SuppressMessage("Sonar",
		"S5443: Temporary files should not be created in publicly writable directories",
		Justification = "Check to not allow this as input for the storage folder mapping")]
	private static readonly IReadOnlyList<string> RestrictedPaths =
	[
		// Linux / shared Unix
		"/bin", "/boot", "/dev", "/etc", "/lib", "/lib64",
		"/proc", "/root", "/run", "/sbin", "/sys", "/usr/bin",
		"/usr/sbin", "/opt", "/var", "/tmp",
		// macOS (canonical forms under /private)
		"/System", "/Library",
		"/private/etc", "/private/var", "/private/tmp",
		// Windows
		@"C:\Windows",
		@"C:\Program Files",
		@"C:\Program Files (x86)",
		@"C:\ProgramData",
		@"C:\System Volume Information"
	];

	// Parent folders of user data: mapping the folder itself would expose every user or
	// mount, but the sub folders (e.g. /home/user/pictures, /mnt/archive) are normal targets.
	[SuppressMessage("Sonar",
		"S1075: Refactor your code not to use hardcoded absolute paths or URIs",
		Justification = "Check to not allow this as input for the storage folder mapping")]
	private static readonly IReadOnlyList<string> RestrictedExactPaths =
		["/home", "/Users", "/mnt", "/media", "/Volumes", @"C:\Users"];

	internal static bool IsRestrictedPath(string physicalPath)
	{
		string canonical;
		try
		{
			canonical = Path.GetFullPath(physicalPath.TrimEnd('/', '\\'));
		}
		catch
		{
			return true;
		}

		var comparison = Path.DirectorySeparatorChar == '\\'
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		return RestrictedExactPaths.Any(restricted => canonical.Equals(restricted, comparison)) ||
		       RestrictedPaths.Any(restricted =>
			       canonical.Equals(restricted, comparison) ||
			       canonical.StartsWith(restricted + Path.DirectorySeparatorChar, comparison));
	}
}
