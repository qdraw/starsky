using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeMocks
{
	public class FakeIQueryException : IQuery
	{
		private readonly Exception _exception;

		public FakeIQueryException(Exception exception)
		{
			_exception = exception;
		}
		public List<FileIndexItem> GetAllFiles(string subPath)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllFilesAsync(List<string> filePaths)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllFilesAsync(string subPath)
		{
			throw _exception;
		}

		public List<FileIndexItem> GetAllRecursive(string subPath = "/")
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllRecursiveAsync(string subPath = "/")
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllRecursiveAsync(List<string> filePathList)
		{
			throw _exception;
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/",
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true,
			bool hideDeleted = true)
		{
			throw _exception;
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(List<FileIndexItem> fileIndexItems,
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true,
			bool hideDeleted = true)
		{
			throw _exception;
		}

		public DetailView SingleItem(string singleItemDbPath, List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true,
			SortType sort = SortType.FileName)
		{
			throw _exception;
		}

		public DetailView SingleItem(List<FileIndexItem> fileIndexItemsList, string singleItemDbPath,
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true,
			bool hideDeleted = true, SortType sort = SortType.FileName)
		{
			throw _exception;
		}

		public FileIndexItem GetObjectByFilePath(string filePath)
		{
			throw _exception;
		}

		public Task<FileIndexItem> GetObjectByFilePathAsync(string filePath, TimeSpan? cacheTime = null)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> inputFilePaths, bool collections)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathQueryAsync(List<string> filePathList)
		{
			throw _exception;
		}

		public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public bool RemoveCacheParentItem(string directoryName)
		{
			throw _exception;
		}

		public string GetSubPathByHash(string fileHash)
		{
			throw _exception;
		}

		public Task<string> GetSubPathByHashAsync(string fileHash)
		{
			return Task.FromResult(GetSubPathByHash(fileHash));
		}

		public Task<List<FileIndexItem>> GetObjectsByFileHashAsync(List<string> fileHashesList)
		{
			throw _exception;
		}

		public void ResetItemByHash(string fileHash)
		{
			throw _exception;
		}

		public List<FileIndexItem> GetAllFolders()
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetFoldersAsync(string subPath)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> GetAllObjectsAsync(
			List<string> filePaths, int fallbackDelay = 5000)
		{
			throw _exception;
		}

		public FileIndexItem AddItem(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList)
		{
			throw _exception;
		}

		public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public List<FileIndexItem> UpdateItem(List<FileIndexItem> updateStatusContentList)
		{
			throw _exception;
		}

		public Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public Task<List<FileIndexItem>> UpdateItemAsync(List<FileIndexItem> updateStatusContentList)
		{
			throw _exception;
		}

		public string SubPathSlashRemove(string subPath = "/")
		{
			throw _exception;
		}

		public RelativeObjects GetNextPrevInFolder(string currentFolder)
		{
			throw _exception;
		}

		public bool AddCacheParentItem(string directoryName, List<FileIndexItem> items)
		{
			throw _exception;
		}

		public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
		{
			throw _exception;
		}

		public void RemoveCacheItem(List<FileIndexItem> updateStatusContent)
		{
			throw _exception;
		}

		public void RemoveCacheItem(FileIndexItem updateStatusContent)
		{
			throw _exception;
		}

		public Tuple<bool, List<FileIndexItem>> CacheGetParentFolder(string subPath)
		{
			throw _exception;
		}

		public Task AddParentItemsAsync(string subPath)
		{
			throw _exception;
		}

		public IQuery Clone(ApplicationDbContext applicationDbContext)
		{
			throw _exception;
		}

		public void Invoke(ApplicationDbContext applicationDbContext)
		{
			throw _exception;
		}

		public void SetGetObjectByFilePathCache(string filePath, FileIndexItem result,
			TimeSpan? cacheTime)
		{
			throw _exception;
		}
		
		public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}
	}
	
}
