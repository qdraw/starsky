using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace starskycore.Helpers
{
    public static class PathHelper
    {

	    /// <summary>
	    /// Return value (works for POSIX/Windows paths)
	    /// </summary>
	    /// <param name="filePath"></param>
	    /// <returns></returns>
	    public static string GetFileName(string filePath)
	    {
			// unescaped:
			// [^/]+(?=(?:\.[^.]+)?$)
			return Regex.Match(filePath, "[^/]+(?=(?:\\.[^.]+)?$)").Value;
	    }

		/// <summary>
		/// Removes the latest backslash. Path.DirectorySeparatorChar
		/// </summary>
		/// <param name="basePath">The base path.</param>
		/// <returns></returns>
		public static string RemoveLatestBackslash(string basePath = "/")
        {
            if (string.IsNullOrWhiteSpace(basePath)) return null;

            // Depends on Platform
            if (basePath == "/") return basePath;
            
            // remove latest backslash
            if (basePath.Substring(basePath.Length - 1, 1) == Path.DirectorySeparatorChar.ToString())
            {
                basePath = basePath.Substring(0, basePath.Length - 1);
            }
            return basePath;
        }

		/// <summary>
		/// Removes the latest slash. (/)
		/// </summary>
		/// <param name="basePath">The base path.</param>
		/// <returns></returns>
		public static string RemoveLatestSlash(string basePath)
		{
			// don't know why it returns / > string.empty

			// on all platforms the same
            if (string.IsNullOrWhiteSpace(basePath) || basePath == "/" ) return string.Empty;

            // remove latest slash
            if (basePath.Substring(basePath.Length - 1, 1) == "/")
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
        public static string AddBackslash(string thumbnailTempFolder) { 
            // Add backSlash to configuration // or \\
            // Platform depended feature
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return thumbnailTempFolder;
            
            if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
                1) != Path.DirectorySeparatorChar.ToString())
            {
                thumbnailTempFolder += Path.DirectorySeparatorChar.ToString();
            }
            return thumbnailTempFolder;
        }

	    /// <summary>
	    /// Add slash / => always
	    /// </summary>
	    /// <param name="thumbnailTempFolder"></param>
	    /// <returns>value +/</returns>
        public static string AddSlash(string thumbnailTempFolder) { 
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return thumbnailTempFolder;
            
            if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
                    1) != Path.DirectorySeparatorChar.ToString())
            {
                thumbnailTempFolder += "/";
            }
            return thumbnailTempFolder;
        }
        

	    /// <summary>
	    /// Add / (always) before string
	    /// </summary>
	    /// <param name="thumbnailTempFolder">the subpath</param>
	    /// <returns>/subpath</returns>
        public static string PrefixDbSlash(string thumbnailTempFolder) { 
            // Add normal linux slash to beginning of the configuration
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return "/";
            
            if (thumbnailTempFolder.Substring(0,1) != "/")
            {
                thumbnailTempFolder = "/" + thumbnailTempFolder;
            }
            return thumbnailTempFolder;
        }
        
	    /// <summary>
	    /// Remove / (always) before string
	    /// </summary>
	    /// <param name="subpath">subpath</param>
	    /// <returns>(without slash) subpath</returns>
        public static string RemovePrefixDbSlash(string subpath) { 
            // Remove linux slash to beginning of the configuration
            if (string.IsNullOrWhiteSpace(subpath)) return "/";
            
            if (subpath.Substring(0,1) == "/")
            {
                subpath = subpath.Remove(0, 1);
            }
            return subpath;
        }
        
        /// <summary>
        /// Split a list with devided by dot comma and blank values are removed
        /// </summary>
        /// <param name="f">input filepaths</param>
        /// <returns>string array with sperated strings</returns>
        public static string[] SplitInputFilePaths(string f)
        {
            if (string.IsNullOrEmpty(f)) return new List<string>().ToArray();
            
            // input devided by dot comma and blank values are removed
            var inputFilePaths = f.Split(";".ToCharArray());
            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();

			// Remove duplicates from list
			// have a single slash in front the path
	        HashSet<string> inputHashSet = new HashSet<string>(); 
	        foreach ( var path in inputFilePaths )
	        {
		        var subpath = RemovePrefixDbSlash(path);
		        subpath = PrefixDbSlash(subpath);

		        inputHashSet.Add(subpath);
	        }
            return inputHashSet.ToArray();
        }
      
        
    }
}
