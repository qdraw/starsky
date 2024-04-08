using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace starsky.foundation.platform.Helpers
{
	public static class PathHelper
	{
		/// <summary>
		/// Return value (works for POSIX/Windows paths)
		/// </summary>
		/// <param name="filePath">path to parse</param>
		/// <returns></returns>
		public static string GetFileName(string? filePath)
		{
			if ( string.IsNullOrEmpty(filePath) )
			{
				return string.Empty;
			}

			if ( filePath.Length >= 4095 )
			{
				// why? https://serverfault.com/questions/9546/filename-length-limits-on-linux
				throw new ArgumentException("[PathHelper] FilePath over Unix limits", nameof(filePath));
			}

			var fileName = GetFileNameUnix(filePath.AsSpan());
			return fileName.ToString();
		}

		[SuppressMessage("Style", "IDE0057:Use range operator")]
		[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
		internal static ReadOnlySpan<char> GetFileNameUnix(ReadOnlySpan<char> path)
		{
			var length = GetPathRootUnix(path).Length;
			var num = path.LastIndexOf('/');
			return path.Slice(num < length ? length : num + 1);
		}

		private static ReadOnlySpan<char> GetPathRootUnix(ReadOnlySpan<char> path)
		{
			return !IsPathRootedUnix(path) ? [] : "/".AsSpan();
		}

		private static bool IsPathRootedUnix(ReadOnlySpan<char> path) =>
			path.Length > 0 && path[0] == '/';

		/// <summary>
		/// Removes the latest backslash. Path.DirectorySeparatorChar
		/// </summary>
		/// <param name="basePath">The base path.</param>
		/// <returns></returns>
		public static string? RemoveLatestBackslash(string basePath = "/")
		{
			if ( string.IsNullOrWhiteSpace(basePath) )
			{
				return null;
			}

			// Depends on Platform
			if ( basePath == "/" ) return basePath;

			// remove latest backslash
			if ( basePath.Substring(basePath.Length - 1, 1) ==
			     Path.DirectorySeparatorChar.ToString() )
			{
				basePath = basePath.Substring(0, basePath.Length - 1);
			}

			return basePath;
		}

		/// <summary>
		/// Removes the latest slash. (/) always unix style
		/// </summary>
		/// <param name="basePath">The base path.</param>
		/// <returns></returns>
		public static string RemoveLatestSlash(string basePath)
		{
			// don't know why it returns / > string.empty

			// on all platforms the same
			if ( string.IsNullOrWhiteSpace(basePath) || basePath == "/" ) return string.Empty;

			// remove latest slash
			if ( basePath.Substring(basePath.Length - 1, 1) == "/" )
			{
				basePath = basePath.Substring(0, basePath.Length - 1);
			}

			return basePath;
		}

		/// <summary>
		/// Add backSlash to configuration // or \\
		/// Platform depended feature
		/// </summary>
		/// <param name="thumbnailTempFolder"></param>
		/// <returns></returns>
		public static string AddBackslash(string thumbnailTempFolder)
		{
			// Add backSlash to configuration // or \\
			// Platform depended feature
			if ( string.IsNullOrWhiteSpace(thumbnailTempFolder) ) return thumbnailTempFolder;

			if ( thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
				    1) != Path.DirectorySeparatorChar.ToString() )
			{
				thumbnailTempFolder += Path.DirectorySeparatorChar.ToString();
			}

			return thumbnailTempFolder;
		}

		/// <summary>
		/// Add / to end of file
		/// </summary>
		/// <param name="inputFolder">Input folder path</param>
		/// <returns>value +/</returns>
		public static string AddSlash(string inputFolder)
		{
			if ( string.IsNullOrWhiteSpace(inputFolder) ) return inputFolder;

			if ( inputFolder.Substring(inputFolder.Length - 1, 1) != "/" )
			{
				inputFolder += "/";
			}

			return inputFolder;
		}

		/// <summary>
		/// Add / (always) before string
		/// </summary>
		/// <param name="subPath">the subPath</param>
		/// <returns>/subpath</returns>
		public static string PrefixDbSlash(string subPath)
		{
			// Add normal linux slash to beginning of the configuration
			if ( string.IsNullOrWhiteSpace(subPath) ) return "/";

			if ( subPath.Substring(0, 1) != "/" )
			{
				subPath = "/" + subPath;
			}

			return subPath;
		}

		/// <summary>
		/// Remove / (always) before string
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <returns>(without slash) subPath</returns>
		public static string RemovePrefixDbSlash(string subPath)
		{
			// Remove linux slash to beginning of the configuration
			if ( string.IsNullOrWhiteSpace(subPath) ) return "/";

			if ( subPath.Substring(0, 1) == "/" )
			{
				subPath = subPath.Remove(0, 1);
			}

			return subPath;
		}

		/// <summary>
		/// Split a list with divided by dot comma and blank values are removed
		/// </summary>
		/// <param name="f">input filePaths</param>
		/// <returns>string array with seperated strings</returns>
		public static string[] SplitInputFilePaths(string f)
		{
			if ( string.IsNullOrEmpty(f) )
			{
				return [];
			}

			// input divided by dot comma and blank values are removed
			var inputFilePaths = f.Split(";".ToCharArray());
			inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();

			// Remove duplicates from list
			// have a single slash in front the path
			var inputHashSet = new HashSet<string>();
			foreach ( var path in inputFilePaths )
			{
				var subPath = RemovePrefixDbSlash(path);
				subPath = PrefixDbSlash(subPath);

				inputHashSet.Add(subPath);
			}

			return inputHashSet.ToArray();
		}
	}
}
