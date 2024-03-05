using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	public sealed class StorageHostFullPathFilesystem : IStorage
	{
		private readonly IWebLogger? _logger;

		public StorageHostFullPathFilesystem(IWebLogger? logger = null)
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
			var type = IsFolderOrFile(path);
			if ( type == FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				// when NOT found
				return new StorageInfo
				{
					IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted,
					Size = -1,
					IsDirectory = null
				};
			}

			var lastWrite = type == FolderOrFileModel.FolderOrFileTypeList.File
				? File.GetLastWriteTime(path)
				: Directory.GetLastWriteTime(path);

			var size = type == FolderOrFileModel.FolderOrFileTypeList.File
				? new FileInfo(path).Length
				: -1;

			return new StorageInfo
			{
				IsFolderOrFile = type,
				IsDirectory = type == FolderOrFileModel.FolderOrFileTypeList.Folder,
				Size = size,
				LastWriteTime = lastWrite,
				IsFileSystemReadOnly = TestIfFileSystemIsReadOnly(path, type)
			};
		}

		internal static bool? TestIfFileSystemIsReadOnly(string folderPath,
			FolderOrFileModel.FolderOrFileTypeList type)
		{
			if ( type != FolderOrFileModel.FolderOrFileTypeList.Folder )
			{
				return null;
			}

			try
			{
				var testFilePath = Path.Combine(folderPath, ".test");
				var myFileStream = File.Open(testFilePath, FileMode.OpenOrCreate,
					FileAccess.ReadWrite, FileShare.ReadWrite);
				myFileStream.Flush();
				myFileStream.Close();
				myFileStream.Dispose(); // also flush
				File.Delete(testFilePath);
			}
			catch ( IOException )
			{
				return true;
			}

			return false;
		}

		public void CreateDirectory(string path)
		{
			Directory.CreateDirectory(path);
		}

		public bool FolderDelete(string path)
		{
			if ( !Directory.Exists(path) ) return false;

			foreach ( var directory in Directory.GetDirectories(path) )
			{
				FolderDelete(directory);
			}

			try
			{
				Directory.Delete(path, true);
			}
			catch ( IOException exception )
			{
				_logger?.LogInformation(exception,
					"[FolderDelete] catch-ed IOException");
				Directory.Delete(path, true);
			}
			catch ( UnauthorizedAccessException exception )
			{
				_logger?.LogInformation(exception,
					"[FolderDelete] catch-ed UnauthorizedAccessException");
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
			catch ( Exception exception )
			{
				if ( exception is not (UnauthorizedAccessException
				    or DirectoryNotFoundException) ) throw;

				_logger?.LogError(exception, "[GetAllFilesInDirectory] " +
				                             "catch-ed UnauthorizedAccessException/DirectoryNotFoundException");
				return Array.Empty<string>();
			}

			var imageFilesList = new List<string>();
			foreach ( var file in allFiles )
			{
				// Path.GetExtension uses (.ext)
				// the same check in SingleFile
				// Recruisive >= same check
				// ignore Files with ._ names, this is Mac OS specific
				var isAppleDouble = Path.GetFileName(file).StartsWith("._");
				if ( !isAppleDouble )
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

		/// <summary>
		/// Returns a list of directories // Get list of child folders
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>list of paths and last edited times - default ordered by last edited times</returns>
		public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
		{
			// Tuple > FilePath,Directory.GetLastWriteTime
			var folders = new Queue<KeyValuePair<string, DateTime>>();
			folders.Enqueue(
				new KeyValuePair<string, DateTime>(path, Directory.GetLastWriteTime(path)));
			var folderList = new List<KeyValuePair<string, DateTime>>();
			while ( folders.Count != 0 )
			{
				var (currentFolder, _) = folders.Dequeue();
				try
				{
					var foldersInCurrent = Directory.GetDirectories(currentFolder,
						"*.*", SearchOption.TopDirectoryOnly);
					foreach ( var current in foldersInCurrent )
					{
						var lastEditDate = Directory.GetLastWriteTime(current);
						folders.Enqueue(new KeyValuePair<string, DateTime>(current, lastEditDate));
						if ( lastEditDate.Year != 1 )
						{
							folderList.Add(
								new KeyValuePair<string, DateTime>(current, lastEditDate));
						}
					}
				}
				catch ( Exception exception )
				{
					if ( exception is not (UnauthorizedAccessException
					    or DirectoryNotFoundException) ) throw;
					_logger?.LogError("[StorageHostFullPathFilesystem] Catch-ed " +
					                  "DirectoryNotFoundException/UnauthorizedAccessException => " +
					                  exception.Message);
				}
			}

			return folderList.OrderBy(p => p.Value);
		}

		/// <summary>
		/// Checks if a file is ready
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[SuppressMessage("Performance", "CA1822:Mark members as static")]
		public bool IsFileReady(string path)
		{
			// If the file can be opened for exclusive access it means that the file
			// is no longer locked by another process.
			try
			{
				using var inputStream = File.Open(path, FileMode.Open,
					FileAccess.Read, FileShare.None);
				return inputStream.Length > 0;
			}
			catch ( Exception )
			{
				return false;
			}
		}

		/// <summary>
		/// Read Stream (and keep open)
		/// </summary>
		/// <param name="path">location</param>
		/// <param name="maxRead">how many bytes are read (default all or -1)</param>
		/// <returns>Stream with data (non-disposed)</returns>
		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( !ExistFile(path) ) throw new FileNotFoundException(path);

			try
			{
				var fileStream = new FileStream(path, FileMode.Open,
					FileAccess.Read, FileShare.Read, 4096, true);

				if ( maxRead < 1 )
				{
					return fileStream;
				}

				// to reuse stream please check StreamGetFirstBytes.GetFirstBytesAsync
				// Only for when selecting the first part of the file
				var buffer = new byte[maxRead];
				// ReSharper disable once MustUseReturnValue
				fileStream.Read(buffer, 0, maxRead);
				fileStream.Close(); // see before max read for default setting
				return new MemoryStream(buffer);
			}
			catch ( FileNotFoundException e )
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
			if ( !Directory.Exists(path) && File.Exists(path) )
			{
				// file
				return FolderOrFileModel.FolderOrFileTypeList.File;
			}

			if ( !File.Exists(path) && Directory.Exists(path) )
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
			Directory.Move(fromPath, toPath);
		}

		/// <summary>
		/// Move file on real filesystem
		/// </summary>
		/// <param name="fromPath">inputFileFullPath</param>
		/// <param name="toPath">toFileFullPath</param>
		public bool FileMove(string fromPath, string toPath)
		{
			if ( fromPath == toPath )
			{
				return false;
			}

			File.Move(fromPath, toPath);
			return true;
		}

		/// <summary>
		/// Copy file on real filesystem
		/// </summary>
		/// <param name="fromPath">inputFileFullPath</param>
		/// <param name="toPath">toFileFullPath</param>
		public void FileCopy(string fromPath, string toPath)
		{
			File.Copy(fromPath, toPath);
		}

		public bool FileDelete(string path)
		{
			if ( !File.Exists(path) )
			{
				return false;
			}

			bool LocalRun()
			{
				File.Delete(path);
				return true;
			}

			return RetryHelper.Do(LocalRun, TimeSpan.FromSeconds(2), 5);
		}

		public bool WriteStream(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;

			bool LocalRun()
			{
				stream.Seek(0, SeekOrigin.Begin);

				using ( var fileStream = new FileStream(path,
					       FileMode.Create,
					       FileAccess.Write, FileShare.ReadWrite,
					       4096,
					       FileOptions.Asynchronous) )
				{
					stream.CopyTo(fileStream);
					// fileStream is disposed due using
				}

				stream.Flush();
				stream.Dispose(); // also flush
				return true;
			}

			return RetryHelper.Do(LocalRun, TimeSpan.FromSeconds(1));
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			if ( !stream.CanRead ) return false;

			stream.Seek(0, SeekOrigin.Begin);

			using ( var fileStream = new FileStream(path,
				       FileMode.OpenOrCreate, // <= that's the difference
				       FileAccess.Write, FileShare.ReadWrite,
				       4096,
				       FileOptions.Asynchronous) )
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
				try
				{
					stream.Seek(0, SeekOrigin.Begin);
				}
				catch ( NotSupportedException )
				{
					// HttpConnection.ContentLengthReadStream does not support this
				}

				using ( var fileStream = new FileStream(path, FileMode.Create,
					       FileAccess.Write, FileShare.Read, 4096,
					       FileOptions.Asynchronous | FileOptions.SequentialScan) )
				{
					await stream.CopyToAsync(fileStream);
					await fileStream.FlushAsync();
				}

				try
				{
					await stream.FlushAsync();
				}
				catch ( NotSupportedException )
				{
					// HttpConnection does not support this - Specified method is not supported.
				}

				await stream.DisposeAsync(); // also flush

				return true;
			}

			return await RetryHelper.DoAsync(LocalRun, TimeSpan.FromSeconds(1), 4);
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
			RecurseFind(path, findList);

			return findList.OrderBy(x => x).ToList();
		}

		internal Tuple<string[], string[]> GetFilesAndDirectories(string path)
		{
			try
			{
				var filesArray = Directory.GetFiles(path);
				var directoriesArray = Directory.GetDirectories(path);
				return new Tuple<string[], string[]>(filesArray,
					directoriesArray);
			}
			catch ( Exception exception )
			{
				_logger?.LogInformation($"[StorageHostFullPathFilesystem] " +
				                        $"catch-ed ex: {exception.Message} -  {path}");
				return new Tuple<string[], string[]>(
					new List<string>().ToArray(),
					new List<string>().ToArray()
				);
			}
		}

		/// <summary>
		/// recursive find. (private)
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="list">The list of strings.</param>
		private void RecurseFind(string path, List<string> list)
		{
			var (filesArray, directoriesArray) = GetFilesAndDirectories(path);
			if ( filesArray.Length <= 0 && directoriesArray.Length <= 0 )
			{
				return;
			}

			//I begin with the files, and store all of them in the list
			list.AddRange(filesArray);
			// I then add the directory and recurse that directory,
			// the process will repeat until there are no more files and directories to recurse
			foreach ( var s in directoriesArray )
			{
				list.Add(s);
				RecurseFind(s, list);
			}
		}

		public DateTime SetLastWriteTime(string path, DateTime? dateTime = null)
		{
			if ( dateTime?.Year == null || dateTime.Value.Year <= 2000 )
			{
				dateTime = DateTime.Now;
			}

			var type = IsFolderOrFile(path);

			if ( type == FolderOrFileModel.FolderOrFileTypeList.File )
			{
				File.SetLastWriteTime(path, dateTime.Value);
			}
			else
			{
				Directory.SetLastWriteTime(path, dateTime.Value);
			}

			return dateTime.Value;
		}
	}
}
