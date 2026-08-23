using System.Linq;
using System.Reflection;

namespace starsky.foundation.platform.Helpers;

public static class GitShaAssembly
{
	public static string GitHash(Assembly assembly)
	{
		var assemblyCommitHash = assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(p => p.Key == "GitCommitHash")
			?.Value;

		return string.IsNullOrWhiteSpace(assemblyCommitHash)
			? string.Empty
			: assemblyCommitHash.Trim();
	}
}
