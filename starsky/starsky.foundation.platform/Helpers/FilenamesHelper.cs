using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
{
	public static partial class FilenamesHelper
	{
		/// <summary>
		/// Regex to match if the filename is valid
		/// use the same as in the front-end
		/// pre compiled regex
		/// Regex.IsMatch
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			"^[a-zA-Z0-9_](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\\.[a-zA-Z0-9_-]+$",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 300)]
		private static partial Regex ValidFileNameRegex();

		/// <summary>
		/// Is the filename valid (WITHOUT parent path)
		/// </summary>
		/// <param name="filename">filename without path</param>
		/// <returns>true when valid</returns>
		public static bool IsValidFileName(string filename)
		{
			// use the same as in the front-end
			return ValidFileNameRegex().IsMatch(filename);
		}

		public delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);

		/// <summary>
		/// Get the filename (with extension) from a filepath
		/// </summary>
		/// <param name="filePath">unix style subPath</param>
		/// <param name="runtimeInformationIsOsPlatform">use to test string replacer under unix</param>
		/// <returns>filename with extension and without its parent path</returns>
		public static string GetFileName(string filePath,
			IsOsPlatformDelegate? runtimeInformationIsOsPlatform = null)
		{
			runtimeInformationIsOsPlatform ??= RuntimeInformation.IsOSPlatform;
			if ( !runtimeInformationIsOsPlatform(OSPlatform.Windows) )
			{
				return Path.GetFileName(filePath);
			}

			const string magicString = "!@&#$#";
			var systemPath = filePath.Replace("\\", magicString).Replace("/",
				Path.DirectorySeparatorChar.ToString());
			return Path.GetFileName(systemPath)
				.Replace(Path.DirectorySeparatorChar.ToString(), "/")
				.Replace(magicString, "\\");
		}

		/// <summary>
		/// Regex to get FileName Without Extension
		/// pre compiled regex
		/// Regex.Match
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			"\\.[a-zA-Z0-9]{1,4}$",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 100)]
		private static partial Regex FileNameWithoutExtensionRegex();

		/// <summary>
		/// Get the filename without extension from a unix path
		/// File dont need to have an extension
		/// </summary>
		/// <param name="filePath">subPath unix style </param>
		/// <returns>filename without extension</returns>
		public static string GetFileNameWithoutExtension(string filePath)
		{
			var fileName = GetFileName(filePath);
			return FileNameWithoutExtensionRegex().Replace(fileName, string.Empty);
		}

		/// <summary>
		/// Regex to File Extension Without Dot
		/// pre compiled regex
		/// Regex.Match
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			"[^.][a-zA-Z0-9]{1,4}$",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 100)]
		private static partial Regex FileExtensionWithoutDotRegex();

		/// <summary>
		/// Get File Extension without dot
		/// </summary>
		/// <param name="filename">fileName or filepath</param>
		/// <returns></returns>
		public static string GetFileExtensionWithoutDot(string filename)
		{
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if ( !filename.Contains('.') )
			{
				return string.Empty;
			}

			return FileExtensionWithoutDotRegex().Match(filename).Value.ToLowerInvariant();
		}

		/// <summary>
		/// Get Parent Regex
		/// unescaped regex: /.+(?=\/[^/]+$)/
		/// pre compiled regex
		/// Regex.Match
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			".+(?=\\/[^/]+$)",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 100)]
		private static partial Regex ParentPathRegex();

		/// <summary>
		/// Return UNIX style parent paths back
		/// </summary>
		/// <param name="filePath">unix style subPath</param>
		/// <returns>parent folder path</returns>
		public static string GetParentPath(string? filePath)
		{
			if ( string.IsNullOrEmpty(filePath) )
			{
				return "/";
			}

			var result = ParentPathRegex().Match(filePath).Value;
			return string.IsNullOrEmpty(result) ? "/" : PathHelper.RemoveLatestSlash(result);
		}
	}
}
