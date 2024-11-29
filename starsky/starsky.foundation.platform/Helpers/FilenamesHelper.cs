using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static partial class FilenamesHelper
{
	public delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);

	/// <summary>
	///     Regex to match if the filename is valid
	///     use the same as in the front-end
	///     pre compiled regular expression
	///     Regex.IsMatch
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		"^[a-zA-Z0-9_](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\\.[a-zA-Z0-9_-]+$",
		RegexOptions.CultureInvariant | RegexOptions.Singleline,
		300)]
	private static partial Regex ValidFileNameRegex();

	/// <summary>
	///     Is the filename valid (WITHOUT parent path)
	/// </summary>
	/// <param name="filename">filename without path</param>
	/// <returns>true when valid</returns>
	public static bool IsValidFileName(string filename)
	{
		// use the same as in the front-end
		return ValidFileNameRegex().IsMatch(filename);
	}

	/// <summary>
	///     Get the filename (with extension) from a filepath
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
	///     Regex to get FileName Without Extension
	///     pre compiled regular expression
	///     Regex.Match
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		"\\.[a-zA-Z0-9]{1,4}$",
		RegexOptions.None,
		100)]
	private static partial Regex FileNameWithoutExtensionRegex();

	/// <summary>
	///     Get the filename without extension from a unix path
	///     File don't need to have an extension
	/// </summary>
	/// <param name="filePath">subPath unix style </param>
	/// <returns>filename without extension</returns>
	public static string GetFileNameWithoutExtension(string filePath)
	{
		var fileName = GetFileName(filePath);
		return FileNameWithoutExtensionRegex().Replace(fileName, string.Empty);
	}

	/// <summary>
	///     Regex to File Extension Without Dot
	///     pre compiled regular expression
	///     Regex.Match
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		"[^.][a-zA-Z0-9]{1,4}$",
		RegexOptions.None,
		100)]
	private static partial Regex FileExtensionWithoutDotRegex();

	/// <summary>
	///     Get File Extension without dot
	/// </summary>
	/// <param name="filename">fileName or filepath</param>
	/// <returns></returns>
	public static string GetFileExtensionWithoutDot(string filename)
	{
		var uri = new Uri(filename, UriKind.RelativeOrAbsolute);
		var path = uri.IsAbsoluteUri
			? uri.LocalPath
			: uri.OriginalString.Split('?')[0].Split('#')[0];

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if ( !path.Contains('.') )
		{
			return string.Empty;
		}

		return FileExtensionWithoutDotRegex().Match(path).Value.ToLowerInvariant();
	}

	/// <summary>
	///     Return UNIX style parent paths back
	///     Get Parent regular expression
	///     unescaped regex: /.+(?=\/[^/]+$)/
	/// </summary>
	/// <param name="filePath">unix style subPath</param>
	/// <returns>parent folder path</returns>
	public static string GetParentPath(string? filePath)
	{
		if ( string.IsNullOrEmpty(filePath) )
		{
			return "/";
		}

		var parts = filePath.TrimEnd('/').Split('/');
		if ( parts.Length <= 2 )
		{
			return "/";
		}

		var stringBuilder = new StringBuilder();
		for ( var i = 0; i < parts.Length - 1; i++ )
		{
			stringBuilder.Append(parts[i] + "/");
		}

		var result = stringBuilder.ToString().TrimEnd('/');

		return string.IsNullOrEmpty(result) ? "/" : PathHelper.RemoveLatestSlash(result);
	}
}
