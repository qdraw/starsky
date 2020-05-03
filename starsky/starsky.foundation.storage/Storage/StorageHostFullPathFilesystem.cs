using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageHostFullPathFilesystem : IStorage
	{

		/// <summary>
		/// Get the storage info
		/// </summary>
		/// <param name="path">full File Path</param>
		/// <returns>StorageInfo object</returns>
		public StorageInfo Info(string path)
		{
			if ( !ExistFile(path) )
			{
				return new StorageInfo
				{
					IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted,
				};
			}
			
			return new StorageInfo
			{
				IsFolderOrFile = IsFolderOrFile(path),
				Length = new FileInfo(path).Length,
			};
		}
		
		public void CreateDirectory(string path)
		{
			throw new System.NotImplementedException();
		}

		public bool FolderDelete(string path)
		{
			foreach (string directory in Directory.GetDirectories(path))
			{
				FolderDelete(directory);
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
			return true;
		}

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

		public IEnumerable<string> GetDirectories(string path)
		{
			return Directory.GetDirectories(path);
		}

		public IEnumerable<string> GetDirectoryRecursive(string path)
		{
			var folders = new Queue<string>();
			folders.Enqueue(path);
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

		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( ! ExistFile(path) ) throw new FileNotFoundException(path);

			FileStream fileStream;
			if ( maxRead <= 1 )
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,true );
			}
			else
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
					FileShare.Read, maxRead, true);
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
		/// <param name="path">fullFilePath</param>
		/// <returns>is file, folder or deleted</returns>
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
		{
			if (!Directory.Exists(path) && File.Exists(path))
			{
				// file
				return FolderOrFileModel.FolderOrFileTypeList.File;
			}

			if (!File.Exists(path) && Directory.Exists(path))
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
			if ( !stream.CanRead ) return false;

			stream.Seek(0, SeekOrigin.Begin);
			
			using (var fileStream = new FileStream(path, 
				FileMode.Create, 
				FileAccess.Write,FileShare.ReadWrite,
				4096, 
				FileOptions.Asynchronous))
			{
				stream.CopyTo(fileStream);
			}
			stream.Dispose();
			return true;
		}
		
		/// <summary>
		/// Write async and disposed after
		/// </summary>
		/// <param name="stream">fileStream</param>
		/// <param name="path">filePath</param>
		/// <returns>success or fail</returns>
		public async Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;
			using (var fileStream = new FileStream(path, FileMode.Create, 
				FileAccess.Write, FileShare.Read, 4096, 
				FileOptions.Asynchronous | FileOptions.SequentialScan))
			{
				await stream.CopyToAsync(fileStream);
			}
			stream.Dispose();
			return true;
		}

		/// <summary>
		/// Gets the files recursive. (only ExtensionSyncSupportedList types)
		/// </summary>
		/// <param name="path">The full file path.</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
        {
            List<string> findlist = new List<string>();

            /* I begin a recursion, following the order:
             * - Insert all the files in the current directory with the recursion
             * - Insert all subdirectories in the list and rebegin the recursion from there until the end
             */
            RecurseFind( path, findlist );

            // Add filter for file types
            var imageFilesList = new List<string>();
            foreach (var file in findlist)
            {
	            imageFilesList.Add(file);
            }
            
            return imageFilesList;
        }

		/// <summary>
		/// recursive find. (private)
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
