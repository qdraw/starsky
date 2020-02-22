using System.Text.RegularExpressions;

namespace starskycore.Helpers
{
	public static class FilenamesHelper
	{
				
		public static bool IsValidFileName(string filename)
		{
			// use the same as in the front-end
			var extensionRegex =
				new Regex("^[a-zA-Z0-9_](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\\.[a-zA-Z0-9_-]+$",
					RegexOptions.CultureInvariant);

			return extensionRegex.IsMatch(filename);
		}

		/// <summary>
		/// Get the filename (with extension) from a filepath
		/// https://stackoverflow.com/a/40635378
		/// </summary>
		/// <param name="filePath">unix style subPath</param>
		/// <returns>filename with extension and without its parent path</returns>
		public static string GetFileName(string filePath)
		{
			// unescaped regex:
			// [^\/]+(?=\.[\w]+\.$)|[^\/]+$
			var extensionRegex =
				new Regex("[^\\/]+(?=\\.[\\w]+\\.$)|[^\\/]+$",
					RegexOptions.CultureInvariant);
			return extensionRegex.Match(filePath).Value;
		}

		/// <summary>
		/// Get the filename without extension from a unix path
		/// File must have an extension
		/// @see: https://stackoverflow.com/a/15229838
		/// </summary>
		/// <param name="filePath">subPath unix style </param>
		/// <returns>filename without extension</returns>
		public static string GetFileNameWithoutExtension(string filePath)
		{
			// unescaped regex: [^/]*(?=\.[^.]+($|\?))
			var extensionRegex =
				new Regex("[^/]*(?=\\.[^.]+($|\\?))",
					RegexOptions.CultureInvariant);
			return extensionRegex.Match(filePath).Value;
		}
		
		/// <summary>
		/// Return UNIX style parent paths back
		/// </summary>
		/// <param name="filePath">unix style subPath</param>
		/// <returns>parent folder path</returns>
		public static string GetParentPath(string filePath)
		{
			if ( string.IsNullOrEmpty(filePath) ) return "/";
	        
			// unescaped regex: /.+(?=\/[^/]+$)/
			var parentRegex =
				new Regex(".+(?=\\/[^/]+$)",
					RegexOptions.CultureInvariant);
			var result = parentRegex.Match(filePath).Value;
			return  string.IsNullOrEmpty(result) ? "/": PathHelper.AddSlash(result);
		}
	}
}
