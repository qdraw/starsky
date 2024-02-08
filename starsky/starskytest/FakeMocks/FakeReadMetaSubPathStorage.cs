using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeReadMetaSubPathStorage : IReadMetaSubPathStorage
	{
		private readonly FakeReadMeta _readMeta;

		public FakeReadMetaSubPathStorage()
		{
			_readMeta = new FakeReadMeta();
		}
		
		public async Task<FileIndexItem?> ReadExifAndXmpFromFileAsync(string subPath)
		{
			return await _readMeta.ReadExifAndXmpFromFileAsync(subPath);
		}

		public  async Task<List<FileIndexItem>> ReadExifAndXmpFromFileAddFilePathHashAsync(List<string> subPathList,
			List<string>? fileHashes = null)
		{
			return await _readMeta.ReadExifAndXmpFromFileAddFilePathHashAsync(subPathList, fileHashes!);
		}

		public bool? RemoveReadMetaCache(string fullFilePath)
		{
			_readMeta.RemoveReadMetaCache(fullFilePath);
			return true;
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
			_readMeta.UpdateReadMetaCache(objectExifToolModel);
		}
	}
}
