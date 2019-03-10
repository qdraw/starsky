using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
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
	    /// Does file exist (true == exist)
	    /// </summary>
	    /// <param name="fullFilePath">full file path</param>
	    /// <returns>bool true = exist</returns>
	    public static bool ExistFile(string fullFilePath = "")
	    {
		    var isFolderOrFile = IsFolderOrFile(fullFilePath);
		    return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
	    }
	    

	    /// <summary>
        /// Returns a list of directories // Get list of child folders
        /// old name: GetFilesRecursive
        /// </summary>
        /// <param name="fullFilePath">directory</param>
        /// <returns></returns>
	    public static string[] GetDirectoryRecursive(string fullFilePath = "")
        {
            if (!Directory.Exists(fullFilePath)) return new List<string>().ToArray();
            string[] folders = Directory.GetDirectories(fullFilePath, "*", SearchOption.AllDirectories);
            // Used For subfolders

            return folders;
        }

        /// <summary>
        /// Returns a list of Files in a directory (non-recruisive)
        /// only files that are in the extenstion list ExtensionSyncSupportedList
        /// </summary>
        /// <param name="fullFilePath">path on the filesystem</param>
        /// <returns></returns>
        public static string[] GetFilesInDirectory(string fullFilePath)
        {
            if (fullFilePath == null) return Enumerable.Empty<string>().ToArray();

            string[] allFiles = Directory.GetFiles(fullFilePath);

            var imageFilesList = new List<string>();
            foreach (var file in allFiles)
            {
                // Path.GetExtension uses (.ext)
                // the same check in SingleFile
                // Recruisive >= same check
                // GetFilesInDirectory
				var extension = Path.GetExtension(file).ToLower().Replace(".",string.Empty);
				// ignore Files with ._ names, this is Mac OS specific
				var isAppleDouble = Path.GetFileName(file).StartsWith("._");
				if (ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(extension) && !isAppleDouble)
                {
                    imageFilesList.Add(file);
                }
            }

            return imageFilesList.ToArray();
        }


	    /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
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

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

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

//		/// <summary>
//		/// Gets the files recrusive. (only ExtensionSyncSupportedList types)
//		/// </summary>
//		/// <param name="fullFilePath">The full file path.</param>
//		/// <returns></returns>
//		public static IEnumerable<string> GetFilesRecursive(string fullFilePath)
//        {
//            List<string> findlist = new List<string>();
//
//            /* I begin a recursion, following the order:
//             * - Insert all the files in the current directory with the recursion
//             * - Insert all subdirectories in the list and rebegin the recursion from there until the end
//             */
//            RecurseFind( fullFilePath, findlist );
//
//            // Add filter for file types
//            var imageFilesList = new List<string>();
//            foreach (var file in findlist)
//            {
//                // Path.GetExtension uses (.ext)
//                //  GetFilesInDirectory
//                // the same check in SingleFile
//                // Recruisive >= same check
//                var extension = Path.GetExtension(file).ToLower().Replace(".",string.Empty);
//                if (ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(extension))
//                {
//                    imageFilesList.Add(file);
//                }
//            }
//            
//            return imageFilesList;
//        }
//
//		/// <summary>
//		/// Recurses the find. (private)
//		/// </summary>
//		/// <param name="path">The path.</param>
//		/// <param name="list">The list of strings.</param>
//		private static void RecurseFind( string path, List<string> list )
//        {
//            string[] fl = Directory.GetFiles(path);
//            string[] dl = Directory.GetDirectories(path);
//            if ( fl.Length>0 || dl.Length>0 )
//            {
//                //I begin with the files, and store all of them in the list
//                foreach(string s in fl)
//                    list.Add(s);
//                // I then add the directory and recurse that directory,
//                // the process will repeat until there are no more files and directories to recurse
//                foreach(string s in dl)
//                {
//                    list.Add(s);
//                    RecurseFind(s, list);
//                }
//            }
//        }

    }
}
