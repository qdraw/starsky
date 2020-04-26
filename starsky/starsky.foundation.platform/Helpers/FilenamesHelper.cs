using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
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
		/// File dont need to have an extension
		/// </summary>
		/// <param name="filePath">subPath unix style </param>
		/// <returns>filename without extension</returns>
		public static string GetFileNameWithoutExtension(string filePath)
		{
			var fileName = GetFileName(filePath);
			return  Regex.Replace(fileName, "\\.[a-zA-Z0-9]{1,4}$", string.Empty );
		}
		
		/// <summary>
		/// Get File Extension without dot
		/// </summary>
		/// <param name="filename">fileName or filepath</param>
		/// <returns></returns>
		public static string GetFileExtensionWithoutDot(string filename)
		{
			if ( !filename.Contains(".") ) return string.Empty;
			return Regex.Match(filename, @"[^.][a-zA-Z0-9]{1,4}$").Value.ToLowerInvariant();
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
			return  string.IsNullOrEmpty(result) ? "/" : PathHelper.RemoveLatestSlash(result);
		}
	}
}
