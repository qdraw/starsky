using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageHostFullPathFilesystem : IStorage
	{
		private readonly IWebLogger _logger;

		public StorageHostFullPathFilesystem(IWebLogger logger = null)
		{
			_logger = logger;
		}

		/// <summary>
		/// Get the storage info
		/// </summary>
		/// <param name="path">full File Path</param>
		/// <returns>StorageInfo object</returns>
		public StorageInfo Info(string path)
		{
			if ( !ExistFile(path) )
			{
				// when NOT found
				return new StorageInfo
				{
					IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted,
					Size = -1
				};
			}
			
			return new StorageInfo
			{
				IsFolderOrFile = IsFolderOrFile(path),
				Size = new FileInfo(path).Length,
				LastWriteTime = File.GetLastWriteTime(path).ToUniversalTime()
			};
		}
		
		public void CreateDirectory(string path)
		{
			Directory.CreateDirectory(path);
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
			catch (IOException exception) 
			{
				_logger?.LogInformation(exception, "[FolderDelete] catch-ed IOException");
				Directory.Delete(path, true);
			}
			catch (UnauthorizedAccessException exception)
			{
				_logger?.LogInformation(exception, "[FolderDelete] catch-ed UnauthorizedAccessException");
				Directory.Delete(path, true);
			}
			return true;
		}

		/// <summary>
		/// Get All files from the directory
		/// </summary>
		/// <param name="path">fullFilePath</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			string[] allFiles;
			try
			{
				 allFiles = Directory.GetFiles(path);
			}
			catch ( UnauthorizedAccessException e )
			{
				_logger?.LogError(e, "[GetAllFilesInDirectory] catch-ed UnauthorizedAccessException");
				return new string[]{};
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

		/// <summary>
		/// List of child directories ordered by name
		/// </summary>
		/// <param name="path">full path on disk</param>
		/// <returns>list child directories in full paths style</returns>
		public IEnumerable<string> GetDirectories(string path)
		{
			return Directory.GetDirectories(path).OrderBy(p => p);
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
					_logger?.LogError("Catch-ed UnauthorizedAccessException => " + e.Message);
				}
			}
			return folderList.OrderBy(p => p);
		}

		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( ! ExistFile(path) ) throw new FileNotFoundException(path);

			FileStream fileStream;
			try
			{
				fileStream = new FileStream(path, FileMode.Open, 
					FileAccess.Read, FileShare.Read, 4096,true );

				if ( maxRead < 1 ) return fileStream;
				
				byte[] buffer = new byte[maxRead];
				fileStream.Read(buffer, 0, maxRead);
				fileStream.Close();
				return new MemoryStream(buffer);
			}
			catch ( FileNotFoundException e)
			{
				_logger?.LogError(e, "[ReadStream] catch-ed FileNotFoundException");
				return Stream.Null;
			}
			
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
		/// is the subPath a folder or file, or deleted (FolderOrFileModel.FolderOrFileTypeList.Deleted)
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
		
		/// <summary>
		/// Move folder on disk
		/// </summary>
		/// <param name="fromPath">inputFileFullPath</param>
		/// <param name="toPath">toFileFullPath</param>
		public void FolderMove(string fromPath, string toPath)
		{
			Directory.Move(fromPath,toPath);
		}
	
		/// <summary>
		/// Move file on real filesystem
		/// </summary>
		/// <param name="fromPath">inputFileFullPath</param>
		/// <param name="toPath">toFileFullPath</param>
		public void FileMove(string fromPath, string toPath)
		{
			File.Move(fromPath,toPath);
		}
		
		/// <summary>
		/// Copy file on real filesystem
		/// </summary>
		/// <param name="fromPath">inputFileFullPath</param>
		/// <param name="toPath">toFileFullPath</param>
		public void FileCopy(string fromPath, string toPath)
		{
			File.Copy(fromPath,toPath);
		}

		public bool FileDelete(string path)
		{
			if ( !File.Exists(path) ) return false;
			bool LocalRun()
			{
				File.Delete(path);
				return true;
			}
			return RetryHelper.Do(LocalRun, TimeSpan.FromSeconds(2),5);
		}

		public bool WriteStream(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;

			bool LocalRun()
			{
				stream.Seek(0, SeekOrigin.Begin);
			
				using (var fileStream = new FileStream(path, 
					FileMode.Create, 
					FileAccess.Write,FileShare.ReadWrite,
					4096, 
					FileOptions.Asynchronous))
				{
					stream.CopyTo(fileStream);
					fileStream.Dispose();
				}

				stream.Dispose();
				return true;
			}
						
			return RetryHelper.Do(LocalRun, TimeSpan.FromSeconds(1));
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;

			stream.Seek(0, SeekOrigin.Begin);
			
			using (var fileStream = new FileStream(path, 
				FileMode.OpenOrCreate, // <= that's the difference
				FileAccess.Write,FileShare.ReadWrite,
				4096, 
				FileOptions.Asynchronous))
			{
				stream.CopyTo(fileStream);
			}
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

			async Task<bool> LocalRun()
			{
				stream.Seek(0, SeekOrigin.Begin);
				using (var fileStream = new FileStream(path, FileMode.Create, 
					FileAccess.Write, FileShare.Read, 4096, 
					FileOptions.Asynchronous | FileOptions.SequentialScan))
				{
					await stream.CopyToAsync(fileStream);
					fileStream.Dispose();
				}
				stream.Dispose();
				return true;
			}

			return await RetryHelper.DoAsync(LocalRun, TimeSpan.FromSeconds(1));
		}

		/// <summary>
		/// Gets the files recursive. (only ExtensionSyncSupportedList types)
		/// </summary>
		/// <param name="path">The full file path.</param>
		/// <returns></returns>
		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
        {
            var findList = new List<string>();

            /* I begin a recursion, following the order:
             * - Insert all the files in the current directory with the recursion
             * - Insert all subdirectories in the list and re-begin the recursion from there until the end
             */
            RecurseFind( path, findList );
            
            return findList.OrderBy(x => x).ToList();
        }

		/// <summary>
		/// recursive find. (private)
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="list">The list of strings.</param>
		private static void RecurseFind( string path, List<string> list )
        {
            var fl = Directory.GetFiles(path);
            var dl = Directory.GetDirectories(path);
            if ( fl.Length <= 0 && dl.Length <= 0 ) return;
            //I begin with the files, and store all of them in the list
            list.AddRange(fl);
            // I then add the directory and recurse that directory,
            // the process will repeat until there are no more files and directories to recurse
            foreach(var s in dl)
            {
	            list.Add(s);
	            RecurseFind(s, list);
            }
        }
	}

}
