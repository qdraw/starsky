using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore.Models;

namespace starskycore.Helpers
{
	
	/// <summary>
	/// WARNING; class is obsolete
	/// </summary>
	[Obsolete("Will be removed in the 0.2.1 release")] 
    public static class FilesHelper
    {

        /// <summary>
        /// is the subpath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
        /// </summary>
        /// <param name="fullFilePath">path of the filesystem</param>
        /// <returns>is file, folder or deleted</returns>
        [Obsolete("Will be removed in the 0.2.1 release")] 
        public static FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string fullFilePath = "")
        {

            if (!Directory.Exists(fullFilePath) && File.Exists(fullFilePath))
            {
                // file
                return FolderOrFileModel.FolderOrFileTypeList.File;
            }

            if (!File.Exists(fullFilePath) && Directory.Exists(fullFilePath))
            {
                // Directory
                return FolderOrFileModel.FolderOrFileTypeList.Folder;
            }

            return FolderOrFileModel.FolderOrFileTypeList.Deleted;
        }
	    
	    /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
	    [Obsolete("Will be removed in the 0.2.1 release")] 
	    public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException) 
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

	    [Obsolete("Will be removed in the 0.2.1 release")] 
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [Obsolete("Will be removed in the 0.2.1 release")] 
        public static void DeleteFile(IEnumerable<string> toDeletePaths)
        {
            foreach (var toDelPath in toDeletePaths)
            {
                if (File.Exists(toDelPath))
                {
                    File.Delete(toDelPath);
                }
            }
        }

    }
}
