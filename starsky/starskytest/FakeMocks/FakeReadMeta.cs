using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	public class FakeReadMeta : IReadMeta
	{
		public FileIndexItem ReadExifAndXmpFromFile(string subPath, ExtensionRolesHelper.ImageFormat imageFormat)
		{
			return new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok, 
				Tags = "test", 
				FileHash = "test", 
				FileName = "t", 
				ParentDirectory = "d"
			};
		}

		public FileIndexItem ReadExifAndXmpFromFile(string path)
		{
			return new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "test, fake read meta"
			};
		}

		public FileIndexItem ReadExifAndXmpFromFile(FileIndexItem fileIndexItemWithLocation)
		{
			return fileIndexItemWithLocation;
		}

		public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathArray, 
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

		public void RemoveReadMetaCache(List<string> fullFilePathArray)
		{
			// dont do anything
		}

		public void RemoveReadMetaCache(string fullFilePath)
		{
			// dont do anything
		}

		public void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel)
		{
			// dont do anything
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel)
		{
		}
	}
}
