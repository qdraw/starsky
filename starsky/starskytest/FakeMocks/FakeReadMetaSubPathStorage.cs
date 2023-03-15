using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeReadMetaSubPathStorage : IReadMetaSubPathStorage
	{
		private readonly IReadMeta _readMeta;

		public FakeReadMetaSubPathStorage()
		{
			_readMeta = new FakeReadMeta();
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
