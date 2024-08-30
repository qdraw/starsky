using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class StorageThumbnailFilesystem : IStorage
	{
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public StorageThumbnailFilesystem(AppSettings appSettings, IWebLogger logger)
		{
			_appSettings = appSettings;
			_logger = logger;
		}

		internal string CombinePath(string fileHash)
		{
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if ( fileHash.EndsWith(".jpg") )
			{
				return Path.Combine(_appSettings.ThumbnailTempFolder, fileHash);
			}

			return Path.Combine(_appSettings.ThumbnailTempFolder, fileHash + ".jpg");
		}

		public bool IsFileReady(string path)
		{
			return new StorageHostFullPathFilesystem(_logger).IsFileReady(CombinePath(path));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">FileHash not path</param>
		/// <returns></returns>
		public bool ExistFile(string path)
		{
			return new StorageHostFullPathFilesystem(_logger).ExistFile(CombinePath(path));
		}

		public bool ExistFolder(string path)
		{
			// only for the root folder
			return new StorageHostFullPathFilesystem(_logger).ExistFolder(_appSettings
				.ThumbnailTempFolder);
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
		{
			throw new System.NotImplementedException();
		}

		public void FolderMove(string fromPath, string toPath)
		{
			throw new System.NotImplementedException();
		}

		public void FolderCopy(string fromPath, string toPath)
		{
			throw new NotImplementedException();
		}

		public bool FileMove(string fromPath, string toPath)
		{
			var oldThumbPath = CombinePath(fromPath);
			var newThumbPath = CombinePath(toPath);

			var hostFilesystem = new StorageHostFullPathFilesystem(_logger);

			var existOldFile = hostFilesystem.ExistFile(oldThumbPath);
			var existNewFile = hostFilesystem.ExistFile(newThumbPath);

			if ( !existOldFile || existNewFile )
			{
				return false;
			}

			hostFilesystem.FileMove(oldThumbPath, newThumbPath);
			return true;
		}

		public void FileCopy(string fromPath, string toPath)
		{
			var oldThumbPath = CombinePath(fromPath);
			var newThumbPath = CombinePath(toPath);

			var hostFilesystem = new StorageHostFullPathFilesystem(_logger);

			var existOldFile = hostFilesystem.ExistFile(oldThumbPath);
			var existNewFile = hostFilesystem.ExistFile(newThumbPath);

			if ( !existOldFile || existNewFile )
			{
				return;
			}

			hostFilesystem.FileCopy(oldThumbPath, newThumbPath);
		}

		/// <summary>
		/// Delete single file
		/// </summary>
		/// <param name="path">fileHash</param>
		/// <returns>true when success</returns>
		public bool FileDelete(string path)
		{
			if ( string.IsNullOrEmpty(path) || !ExistFile(path) ) return false;

			var thumbPath = CombinePath(path);
			var hostFilesystem = new StorageHostFullPathFilesystem(_logger);
			return hostFilesystem.FileDelete(thumbPath);
		}

		public void CreateDirectory(string path)
		{
			throw new System.NotImplementedException();
		}

		public bool FolderDelete(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(_appSettings.ThumbnailTempFolder);
			return dirInfo.EnumerateFiles($"*.jpg", SearchOption.TopDirectoryOnly)
				.Select(p => p.Name).ToList();
		}

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetDirectories(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
		{
			throw new NotImplementedException();
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
			return new StorageHostFullPathFilesystem(_logger).ReadStream(CombinePath(path),
				maxRead);
		}

		/// <summary>
		/// To Write the thumbnail stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool WriteStream(Stream stream, string path)
		{
			return new StorageHostFullPathFilesystem(_logger)
				.WriteStream(stream, CombinePath(path));
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			return new StorageHostFullPathFilesystem(_logger).WriteStreamAsync(stream,
				CombinePath(path));
		}

		public StorageInfo Info(string path)
		{
			throw new System.NotImplementedException();
		}
	}
}
