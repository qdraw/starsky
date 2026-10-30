using starsky.foundation.platform.Helpers;

namespace starsky.project.mcp;

internal static class McpPaths
{
	/// <summary>
	///     Same rules as the REST api: leading slash, no trailing slash, root is `/`
	/// </summary>
	internal static string ToSubPath(string? path)
	{
		var subPath = PathHelper.RemoveLatestSlash(PathHelper.PrefixDbSlash(path ?? "/"));
		return string.IsNullOrEmpty(subPath) ? "/" : subPath;
	}
}
