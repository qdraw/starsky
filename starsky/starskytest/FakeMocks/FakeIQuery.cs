using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeMocks
{
	public class FakeIQuery : IQuery
	{
		private List<FileIndexItem> _fakeContext = new List<FileIndexItem>();
		
		public List<FileIndexItem> GetAllFiles(string subPath)
		{
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> GetAllRecursive(string subPath = "")
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(List<FileIndexItem> fileIndexItems, List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public DetailView SingleItem(string singleItemDbPath, List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public DetailView SingleItem(List<FileIndexItem> fileIndexItemsList, string singleItemDbPath,
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public FileIndexItem GetObjectByFilePath(string filePath)
		{
			throw new System.NotImplementedException();
		}

		public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveCacheParentItem(string directoryName)
		{
			throw new System.NotImplementedException();
		}

		public string GetSubPathByHash(string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public void ResetItemByHash(string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> GetAllFolders()
		{
			throw new System.NotImplementedException();
		}

		public FileIndexItem AddItem(FileIndexItem updateStatusContent)
		{
			_fakeContext.Add(updateStatusContent);
			return updateStatusContent;
		}

		public async Task<FileIndexItem> AddItemAsync(FileIndexItem updateStatusContent)
		{
			return AddItem(updateStatusContent);
		}

		public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
		{
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList)
		{
			throw new System.NotImplementedException();
		}

		public string SubPathSlashRemove(string subPath = "/")
		{
			throw new System.NotImplementedException();
		}

		public RelativeObjects GetNextPrevInFolder(string currentFolder)
		{
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList)
		{
			throw new System.NotImplementedException();
		}

		public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
		{
			throw new System.NotImplementedException();
		}

		public bool IsCacheEnabled()
		{
			throw new System.NotImplementedException();
		}
	}
}
