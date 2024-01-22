using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Interfaces;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	public class FakeReadMeta : IReadMeta
	{
		public Task<FileIndexItem> ReadExifAndXmpFromFileAsync(string subPath)
		{
			return Task.FromResult(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "test, fake read meta"
			});
		}

		public Task<List<FileIndexItem>> ReadExifAndXmpFromFileAddFilePathHashAsync(List<string> subPathList,
			List<string> fileHashes = null)
		{
			var createAnImage = new CreateAnImage();
			return Task.FromResult(new List<FileIndexItem>
			{
				new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok, 
					FileName = createAnImage.FileName
				}
			});
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
