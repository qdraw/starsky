using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageHostFullPathFilesystem : IStorage
	{
		public void CreateDirectory(string path)
		{
			throw new System.NotImplementedException();
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetAllFilesInDirectory(string fullFilePath)
		{
			var allFiles = new string[]{};
			try
			{
				 allFiles = Directory.GetFiles(fullFilePath);
			}
			catch ( UnauthorizedAccessException e )
			{
				Console.WriteLine(e);
				return allFiles;
			}

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

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public IEnumerable<string> GetDirectoryRecursive(string fullFilePath)
		{
			var folders = new Queue<string>();
			folders.Enqueue(fullFilePath);
			var folderList = new List<string>();
			while (folders.Count != 0) {
				var currentFolder = folders.Dequeue();
				try
				{
					var foldersInCurrent = Directory.GetDirectories(currentFolder,
						"*.*", SearchOption.TopDirectoryOnly);
					foreach ( var current in foldersInCurrent )
					{
						folders.Enqueue(current);
						if ( Directory.GetLastAccessTime(current).Year != 1 )
						{
							folderList.Add(current);
						}
					}
				}
				catch(UnauthorizedAccessException e) 
				{
					Console.WriteLine(e);
				}
			}
			return folderList;
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
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
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public bool ExistFile(string path)
		{
			var isFolderOrFile = IsFolderOrFile(path);
			return isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File;
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
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
		[Obsolete("do not include direct, only using ISelectorStorage")]
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
		
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void FolderMove(string inputFileFullPath, string toFileFullPath)
		{
			Directory.Move(inputFileFullPath,toFileFullPath);
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		
		public void FileMove(string inputFileFullPath, string toFileFullPath)
		{
			File.Move(inputFileFullPath,toFileFullPath);
		}
		
		[Obsolete("do not include direct, only using ISelectorStorage")]
		public void FileCopy(string fromPath, string toPath)
		{
			File.Copy(fromPath,toPath);
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public bool FileDelete(string path)
		{
			if ( !File.Exists(path) ) return false;
			File.Delete(path);
			return true;
		}

		[Obsolete("do not include direct, only using ISelectorStorage")]
		public bool WriteStream(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;

			stream.Seek(0, SeekOrigin.Begin);
			
			using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				stream.CopyTo(fileStream);
			}
			stream.Dispose();
			return true;
		}
		
		/// <summary>
		/// Write async
		/// </summary>
		/// <param name="stream">fileStream</param>
		/// <param name="path">filePath</param>
		/// <returns>success or fail</returns>
		public async Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;
			using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				await stream.CopyToAsync(fileStream); // changed
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

		public bool ThumbnailDelete(string fileHash)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Gets the files recrusive. (only ExtensionSyncSupportedList types)
		/// </summary>
		/// <param name="fullFilePath">The full file path.</param>
		/// <returns></returns>
		[Obsolete("do not include direct, only using ISelectorStorage")]
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
