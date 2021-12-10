using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.readmeta.Services
{
	[Service(typeof(IReadMetaSubPathStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ReadMetaSubPathStorage : IReadMetaSubPathStorage
	{
		private readonly ReadMeta _readMeta;

		public ReadMetaSubPathStorage(ISelectorStorage selectorStorage, AppSettings appSettings, IMemoryCache memoryCache)
		{
			var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_readMeta = new ReadMeta(storage, appSettings, memoryCache);
		}
		
		public FileIndexItem ReadExifAndXmpFromFile(string subPath)
		{
			return _readMeta.ReadExifAndXmpFromFile(subPath);
		}

		public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList,
			List<string> fileHashes = null)
		{
			return _readMeta.ReadExifAndXmpFromFileAddFilePathHash(subPathList,fileHashes);
		}

		public void RemoveReadMetaCache(string fullFilePath)
		{
			_readMeta.RemoveReadMetaCache(fullFilePath);
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
			_readMeta.UpdateReadMetaCache(objectExifToolModel);
		}
	}
}
