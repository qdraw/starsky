using System;
using System.IO;
using System.Linq;

namespace starsky.foundation.platform.Helpers;

public static class PathTraversalGuard
{
	public static bool ContainsTraversal(string? input)
	{
		if ( string.IsNullOrWhiteSpace(input) )
		{
			return false;
		}

		var normalized = input.Replace('\\', '/');
		var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
		return segments.Any(segment => segment == ".." || segment == ".");
	}

	public static string ToSafeFullPath(string rootPath, string dbPath)
	{
		if ( ContainsTraversal(dbPath) )
		{
			throw new UnauthorizedAccessException("Path traversal detected");
		}

		var rootFullPath = Path.GetFullPath(rootPath);
		var rootWithSeparator = rootFullPath.EndsWith(Path.DirectorySeparatorChar)
			? rootFullPath
			: rootFullPath + Path.DirectorySeparatorChar;

		var normalizedDbPath = dbPath.Replace('\\', '/');
		if ( normalizedDbPath.StartsWith('/') )
		{
			normalizedDbPath = normalizedDbPath[1..];
		}

		var relativePath = normalizedDbPath.Replace('/', Path.DirectorySeparatorChar);
		var fullPath = Path.GetFullPath(Path.Combine(rootFullPath, relativePath));

		if ( fullPath.Equals(rootFullPath, StringComparison.OrdinalIgnoreCase) )
		{
			return fullPath;
		}

		if ( !fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) )
		{
			throw new UnauthorizedAccessException("Path traversal detected");
		}

		return fullPath;
	}
}


