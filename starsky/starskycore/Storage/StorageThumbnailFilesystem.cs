using System.Collections.Generic;
using System.IO;
using starsky.foundation.ioc;
using starskycore.Interfaces;
using starskycore.Models;


namespace starskycore.Services
{
	[Service(typeof(IStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class StorageThumbnailFilesystem : IStorage
	{
		public bool ExistFile(string path)
		{
			throw new System.NotImplementedException();
		}

		public bool ExistFolder(string path)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public void FileCopy(string fromPath, string toPath)
		{
			throw new System.NotImplementedException();
		}

		public bool FileDelete(string path)
		{
			throw new System.NotImplementedException();
		}

		public void CreateDirectory(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> GetDirectoryRecursive(string path)
		{
			throw new System.NotImplementedException();
		}

		public Stream ReadStream(string path, int maxRead = -1)
		{
			throw new System.NotImplementedException();
		}

		public bool WriteStream(Stream stream, string path)
		{
			throw new System.NotImplementedException();
		}

		public bool ThumbnailExist(string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public Stream ThumbnailRead(string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public bool ThumbnailWriteStream(Stream stream, string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public void ThumbnailMove(string fromFileHash, string toFileHash)
		{
			throw new System.NotImplementedException();
		}

		public bool ThumbnailDelete(string fileHash)
		{
			throw new System.NotImplementedException();
		}
	}
}
