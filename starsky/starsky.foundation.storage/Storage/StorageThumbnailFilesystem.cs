using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageThumbnailFilesystem : IStorage
	{
		private readonly AppSettings _appSettings;

		public StorageThumbnailFilesystem(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">FileHash not path</param>
		/// <returns></returns>
		public bool ExistFile(string path)
		{
			var filePath = Path.Combine(_appSettings.ThumbnailTempFolder, path + ".jpg");
			return new StorageHostFullPathFilesystem().ExistFile(filePath);
		}

		public bool ExistFolder(string path)
		{
			// only for the root folder
			return new StorageHostFullPathFilesystem().ExistFolder(_appSettings.ThumbnailTempFolder);
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
		{
			throw new System.NotImplementedException();
		}

		public void FolderMove(string fromPath, string toPath)
		{
			throw new System.NotImplementedException();
		}

		public void FileMove(string fromPath, string toPath)
		{
			var oldThumbPath = _appSettings.ThumbnailTempFolder + fromPath + ".jpg";
			var newThumbPath = _appSettings.ThumbnailTempFolder + toPath + ".jpg";

			var hostFilesystem = new StorageHostFullPathFilesystem();

			var existOldFile = hostFilesystem.ExistFile(oldThumbPath);
			var existNewFile = hostFilesystem.ExistFile(newThumbPath);

			if (!existOldFile || existNewFile)
			{
				return;
			}
			hostFilesystem.FileMove(oldThumbPath,newThumbPath);
		}

		public void FileCopy(string fromPath, string toPath)
		{
			throw new System.NotImplementedException();
		}

		public bool FileDelete(string path)
		{
			if ( !ExistFile(path) ) return false;

			var thumbPath = _appSettings.ThumbnailTempFolder + path + ".jpg";
			var hostFilesystem = new StorageHostFullPathFilesystem();
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

		public IEnumerable<string> GetDirectoryRecursive(string path)
		{
			throw new System.NotImplementedException();
		}

		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( !ExistFile(path) ) throw new FileNotFoundException(path); 
			var filePath = Path.Combine(_appSettings.ThumbnailTempFolder, path + ".jpg");
			return new StorageHostFullPathFilesystem().ReadStream(filePath);
		}

		/// <summary>
		/// To Write the thumbnail stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool WriteStream(Stream stream, string path)
		{
			return new StorageHostFullPathFilesystem()
				.WriteStream(stream, Path.Combine(_appSettings.ThumbnailTempFolder, path + ".jpg"));
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			var fullFilePath = Path.Combine(_appSettings.ThumbnailTempFolder, path + ".jpg");
			return new StorageHostFullPathFilesystem().WriteStreamAsync(stream, fullFilePath);
		}

		public StorageInfo Info(string path)
		{
			throw new System.NotImplementedException();
		}
	}
}
