using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Interfaces;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	public class FakeReadMeta : IReadMeta
	{
		public FileIndexItem ReadExifAndXmpFromFile(string subPath)
		{
			return new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "test, fake read meta"
			};
		}

		public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList, 
			List<string> fileHashes = null)
		{
			var createAnImage = new CreateAnImage();
			return new List<FileIndexItem>
			{
				new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok, 
					FileName = createAnImage.FileName
				}
			};
		}

		public bool? RemoveReadMetaCache(string fullFilePath)
		{
			// dont do anything
			return true;
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
		}
	}
}
