using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.readmeta.Services
{
	[Service(typeof(IReadMetaSubPathStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class ReadMetaSubPathStorage : IReadMetaSubPathStorage
	{
		private readonly ReadMeta _readMeta;

		public ReadMetaSubPathStorage(ISelectorStorage selectorStorage, AppSettings appSettings, IMemoryCache memoryCache, IWebLogger logger)
		{
			var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_readMeta = new ReadMeta(storage, appSettings, memoryCache,logger);
		}
		
		public async Task<List<FileIndexItem>> ReadExifAndXmpFromFileAddFilePathHashAsync(List<string> subPathList,
			List<string>? fileHashes = null)
		{
			return await _readMeta.ReadExifAndXmpFromFileAddFilePathHashAsync(subPathList,fileHashes);
		}

		public async Task<FileIndexItem?> ReadExifAndXmpFromFileAsync(string subPath)
		{
			return await _readMeta.ReadExifAndXmpFromFileAsync(subPath);
		}

		public bool? RemoveReadMetaCache(string fullFilePath)
		{
			return _readMeta.RemoveReadMetaCache(fullFilePath);
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
			_readMeta.UpdateReadMetaCache(objectExifToolModel);
		}
	}
}
