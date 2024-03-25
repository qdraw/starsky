using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
{
	public static partial class PathHelper
	{
		/// <summary>
		/// Regex to match a filename in a path
		/// unescaped:
		/// [^/]+(?=(?:\.[^.]+)?$)
		/// pre compiled regex Regex.Match
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			"[^/]+(?=(?:\\.[^.]+)?$)",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 1000)]
		private static partial Regex GetFileNameRegex();

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

			return GetFileNameRegex().Match(filePath).Value;
		}

		private static string? GetFileNameHelper(string input)
		{
			      // Initialize variables to store the result
        string result = "";
        bool isValid = true;

        // Define variables to keep track of the current position and length of the match
        int startPos = 0;
        int matchLength = 0;

        // Iterate over the characters in the input string
        for (int i = 0; i < input.Length; i++)
        {
            // If the current character is '/', it means the end of a potential match
            if (input[i] == '/')
            {
                // Extract the potential match substring
                string potentialMatch = input.Substring(startPos, matchLength);

                // Check if the potential match matches the pattern [^/]+(?=(?:\.[^.]+)?$)
                if (!potentialMatch.Contains("/") && (potentialMatch.Contains(".") || potentialMatch.EndsWith(".")))
                {
                    // Add the potential match to the result
                    result += potentialMatch + "/";

                    // Update the start position for the next potential match
                    startPos = i + 1;
                    matchLength = 0;
                }
                else
                {
                    // If the potential match does not match the pattern, set isValid to false and break the loop
                    isValid = false;
                    break;
                }
            }
            else
            {
                // Increment the length of the potential match
                matchLength++;
            }
        }

        // Handle the last potential match after the loop
        if (matchLength > 0)
        {
            string lastPotentialMatch = input.Substring(startPos, matchLength);
            if (!lastPotentialMatch.Contains("/") && (lastPotentialMatch.Contains(".") || lastPotentialMatch.EndsWith(".")))
            {
                result += lastPotentialMatch;
            }
            else
            {
                isValid = false;
            }
        }

        // If isValid is true and result is not empty, remove the trailing '/'
        if (isValid && result.Length > 0)
        {
            result = result.TrimEnd('/');
        }
        else
        {
            // If the input string does not match the pattern, set result to null
            result = null;
        }

        // Output the result
        return result;
		}

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
