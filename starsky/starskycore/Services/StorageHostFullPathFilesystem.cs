using System;
using System.Collections.Generic;
using System.IO;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class StorageHostFullPathFilesystem : IStorage
	{
		public void CreateDirectory(string subPath)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetAllFilesInDirectory(string fullFilePath)
		{
			string[] allFiles = Directory.GetFiles(fullFilePath);

			var imageFilesList = new List<string>();
			foreach (var file in allFiles)
			{
				// Path.GetExtension uses (.ext)
				// the same check in SingleFile
				// Recruisive >= same check
				// ignore Files with ._ names, this is Mac OS specific
				var isAppleDouble = Path.GetFileName(file).StartsWith("._");
				if (!isAppleDouble)
				{
					imageFilesList.Add(file);
				}
				// to filter use:
				// ..etAllFilesInDirectory(subPath)
				//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
				// OR:
				//  .Where(ExtensionRolesHelper.IsExtensionSyncSupported
			}

			return imageFilesList;
		}

		public IEnumerable<string> GetDirectoryRecursive(string fullFilePath)
		{
			return Directory.GetDirectories(fullFilePath, "*", SearchOption.AllDirectories);
		}

		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( ! ExistFile(path) ) throw new FileNotFoundException(path);

			FileStream fileStream;
			if ( maxRead <= 1 )
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			}
			else
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
					FileShare.Read, maxRead, false);
			}
			return fileStream;
		}


		/// <summary>
		/// Does file exist (true == exist)
		/// </summary>
		/// <param name="path">full file path</param>
		/// <returns>bool true = exist</returns>
		public bool ExistFile(string path)
		{
			var isFolderOrFile = IsFolderOrFile(path);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
		}

		public bool ExistFolder(string path)
		{
			var isFolderOrFile = IsFolderOrFile(path);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.Folder;
		}

		/// <summary>
		/// is the subpath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
		/// </summary>
		/// <param name="fullFilePath">fullFilePath</param>
		/// <returns>is file, folder or deleted</returns>
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string fullFilePath)
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

		public void FolderMove(string inputFileFullPath, string toFileFullPath)
		{
			Directory.Move(inputFileFullPath,toFileFullPath);
		}

		public void FileMove(string inputFileFullPath, string toFileFullPath)
		{
			File.Move(inputFileFullPath,toFileFullPath);
		}
		
		public void FileCopy(string fromPath, string toPath)
		{
			File.Copy(fromPath,toPath);
		}

		
		public bool FileDelete(string path)
		{
			if ( !File.Exists(path) ) return false;
			File.Delete(path);
			return true;
		}
		
		public bool WriteStream(Stream stream, string path)
		{
			if ( ! ExistFile(path) ) throw new FileNotFoundException(path);
			
			using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				stream.CopyTo(fileStream);
			}
			stream.Dispose();
			return true;
		}

		public bool ThumbnailExist(string fileHash)
		{
			throw new NotImplementedException();
		}

		public Stream ThumbnailRead(string fileHash)
		{
			throw new NotImplementedException();
		}

		public bool ThumbnailWriteStream(Stream stream, string fileHash)
		{
			throw new NotImplementedException();
		}

		public void ThumbnailMove(string fromFileHash, string toFileHash)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Gets the files recrusive. (only ExtensionSyncSupportedList types)
		/// </summary>
		/// <param name="fullFilePath">The full file path.</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string fullFilePath)
        {
            List<string> findlist = new List<string>();

            /* I begin a recursion, following the order:
             * - Insert all the files in the current directory with the recursion
             * - Insert all subdirectories in the list and rebegin the recursion from there until the end
             */
            RecurseFind( fullFilePath, findlist );

            // Add filter for file types
            var imageFilesList = new List<string>();
            foreach (var file in findlist)
            {
	            imageFilesList.Add(file);
//                // Path.GetExtension uses (.ext)
//                //  GetFilesInDirectory
//                // the same check in SingleFile
//                // Recruisive >= same check
//                var extension = Path.GetExtension(file).ToLower().Replace(".",string.Empty);
//                if (ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(extension))
//                {
//                    
//                }
            }
            
            return imageFilesList;
        }

		/// <summary>
		/// Recurses the find. (private)
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="list">The list of strings.</param>
		private static void RecurseFind( string path, List<string> list )
        {
            string[] fl = Directory.GetFiles(path);
            string[] dl = Directory.GetDirectories(path);
            if ( fl.Length>0 || dl.Length>0 )
            {
                //I begin with the files, and store all of them in the list
                foreach(string s in fl)
                    list.Add(s);
                // I then add the directory and recurse that directory,
                // the process will repeat until there are no more files and directories to recurse
                foreach(string s in dl)
                {
                    list.Add(s);
                    RecurseFind(s, list);
                }
            }
        }


	}
}
