using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIQuery : IQuery
	{
		public FakeIQuery(List<FileIndexItem> fakeContext = null, List<FileIndexItem> fakeCachedContent = null)
		{
			if ( fakeContext == null ) return;
			_fakeContext = fakeContext;
			_fakeCachedContent = fakeCachedContent;
		}

		public FakeIQuery(ApplicationDbContext context, 
			AppSettings appSettings,
			IServiceScopeFactory scopeFactory, 
			IWebLogger logger, IMemoryCache memoryCache = null)
		{
		}
		
		private readonly List<FileIndexItem> _fakeContext = new List<FileIndexItem>();
		private List<FileIndexItem> _fakeCachedContent = new List<FileIndexItem>();

		public List<FileIndexItem> GetAllFiles(string subPath)
		{
			return _fakeContext.Where(p => p.ParentDirectory == subPath && p.IsDirectory == false).ToList();
		}

		public Task<List<FileIndexItem>> GetAllFilesAsync(List<string> filePaths)
		{
			var result = new List<FileIndexItem>();
			foreach ( var subPath in filePaths )
			{
				result.AddRange(GetAllFiles(subPath));
			}
			return Task.FromResult(result);
		}

		public Task<List<FileIndexItem>> GetAllFilesAsync(string subPath)
		{
			return Task.FromResult(GetAllFiles(subPath));
		}

		public List<FileIndexItem> GetAllRecursive(string subPath = "")
		{
			return _fakeContext.Where
					(p => p.ParentDirectory.StartsWith(subPath))
				.OrderBy(r => r.FileName).ToList();
		}

		public Task<List<FileIndexItem>> GetAllRecursiveAsync(string subPath = "/")
		{
			return Task.FromResult(GetAllRecursive(subPath));
		}
		
		public Task<List<FileIndexItem>> GetAllRecursiveAsync(List<string> filePathList)
		{
			var result = new List<FileIndexItem>();
			foreach ( var subPath in filePathList )
			{
				result.AddRange(GetAllRecursive(subPath));
			}
			return Task.FromResult(result);
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", 
			List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			return GetAllFiles(subPath);
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(List<FileIndexItem> fileIndexItems, List<ColorClassParser.Color> 
				colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public DetailView SingleItem(string singleItemDbPath, List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true, 
			SortType sort = SortType.FileName)
		{
			if ( _fakeContext.All(p => p.FilePath != singleItemDbPath) )
			{
				return null;
			}

			var fileIndexItem = _fakeContext.FirstOrDefault(p => p.FilePath == singleItemDbPath);
			if ( fileIndexItem == null ) return null;
			fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			fileIndexItem.CollectionPaths = new List<string>{singleItemDbPath};
			if ( enableCollections )
			{
				fileIndexItem.CollectionPaths = new List<string>();
				fileIndexItem.CollectionPaths.AddRange(
					_fakeContext.Where(
							p => p.FileCollectionName == fileIndexItem.FileCollectionName)
						.Select(p => p.FilePath)
				);
			}
			return new DetailView {FileIndexItem = fileIndexItem, IsDirectory = fileIndexItem.IsDirectory == true};
		}

		public DetailView SingleItem(List<FileIndexItem> fileIndexItemsList, string singleItemDbPath,
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true, bool hideDeleted = true, 
			SortType sort = SortType.FileName)
		{
			throw new System.NotImplementedException();
		}

		public FileIndexItem GetObjectByFilePath(string filePath)
		{
			return _fakeContext.FirstOrDefault(p => p.FilePath == filePath);
		}

		public async Task<FileIndexItem> GetObjectByFilePathAsync(string filePath, TimeSpan? cacheTime = null)
		{
			try
			{
				return _fakeContext.FirstOrDefault(p => p.FilePath == filePath);
			}
			catch (InvalidOperationException)
			{
				await Task.Delay(new Random().Next(1, 5));
				return _fakeContext.FirstOrDefault(p => p.FilePath == filePath);
			}
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> filePathList)
		{
			var result = new List<FileIndexItem>();
			foreach ( var filePath in filePathList )
			{
				result.AddRange(_fakeContext.Where(p=> p.FilePath == filePath));
			}
			return Task.FromResult(result);
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> inputFilePaths, bool collections)
		{
			if ( collections )
			{
				return GetObjectsByFilePathCollectionAsync(
					inputFilePaths.ToList());
			}
			return GetObjectsByFilePathAsync(inputFilePaths.ToList());
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathQueryAsync(List<string> filePathList)
		{
			return GetObjectsByFilePathAsync(filePathList);
		}

		public Task<List<FileIndexItem>> GetObjectsByFilePathCollectionAsync(List<string> filePathList)
		{
			var result = new List<FileIndexItem>();
			foreach ( var path in filePathList )
			{
				var fileNameWithoutExtension = FilenamesHelper.GetFileNameWithoutExtension(path);
				result.AddRange(_fakeContext.Where(p => p.ParentDirectory == FilenamesHelper.GetParentPath(path) 
					&& p.FileName.StartsWith(fileNameWithoutExtension)));
			}
			return Task.FromResult(result);
		}

		public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
		{
			_fakeContext.Remove(updateStatusContent);
			return updateStatusContent;
		}

		public async Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent)
		{
			try
			{
				_fakeContext.Remove(updateStatusContent);
			}
			catch ( ArgumentOutOfRangeException )
			{
				await Task.Delay(new Random().Next(1, 5));
				_fakeContext.Remove(updateStatusContent);
			}
			return updateStatusContent;
		}

		public bool RemoveCacheParentItem(string directoryName)
		{
			if ( _fakeCachedContent == null ) return false;
			var item = _fakeCachedContent.FirstOrDefault(p =>
				p.ParentDirectory == directoryName);
			_fakeCachedContent.Remove(item);
			return true;
		}

		public string GetSubPathByHash(string fileHash)
		{
			return _fakeContext.FirstOrDefault(p => p.FileHash == fileHash)?.FilePath;
		}

		public Task<string> GetSubPathByHashAsync(string fileHash)
		{
			return Task.FromResult(GetSubPathByHash(fileHash));
		}

		public Task<List<FileIndexItem>> GetObjectsByFileHashAsync(List<string> fileHashesList)
		{
			var result = _fakeContext.Where(p =>
					fileHashesList.Contains(p.FileHash)).ToList();
			
			foreach ( var fileHash in fileHashesList )
			{
				if ( result.FirstOrDefault(p => p.FileHash == fileHash) == null )
				{
					result.Add(new FileIndexItem(){FileHash = fileHash, 
						Status = FileIndexItem.ExifStatus.NotFoundNotInIndex});
				}
			}
			
			return Task.FromResult(result);
		}

		public void ResetItemByHash(string fileHash)
		{
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> GetAllFolders()
		{
			return _fakeContext.Where(p => p.IsDirectory == true).ToList();
		}

		public Task<List<FileIndexItem>> GetFoldersAsync(string subPath)
		{
			return Task.FromResult(_fakeContext.Where(p => p.ParentDirectory == subPath && p.IsDirectory == true).ToList());
		}

		public Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath)
		{
			return Task.FromResult(_fakeContext.Where(p => p.ParentDirectory == subPath).ToList());
		}

		public async Task<List<FileIndexItem>> GetAllObjectsAsync(
			List<string> filePaths, int fallbackDelay = 5000)
		{
			var result = new List<FileIndexItem>();
			foreach ( var subPath in filePaths )
			{
				result.AddRange(await GetAllObjectsAsync(subPath));
			}
			return result;
		}

		public FileIndexItem AddItem(FileIndexItem updateStatusContent)
		{
			_fakeContext.Add(updateStatusContent);
			return updateStatusContent;
		}

		public async Task<FileIndexItem> AddItemAsync(FileIndexItem updateStatusContent)
		{
			_fakeContext.Add(updateStatusContent);
			await Task.Delay(new Random().Next(1, 5));
			if ( _fakeContext.FirstOrDefault(p => 
				p.FilePath == updateStatusContent.FilePath) != null ) return updateStatusContent;
			
			_fakeContext.Add(updateStatusContent);
			return updateStatusContent;
		}

		public async Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList)
		{
			foreach ( var fileIndexItem in fileIndexItemList )
			{
				await AddItemAsync(fileIndexItem);
			}
			return fileIndexItemList;
		}

		public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
		{
			var item = _fakeContext.FirstOrDefault(p =>
				p.FilePath == updateStatusContent.FilePath);
			if ( item == null ) return updateStatusContent;
			var index = _fakeContext.IndexOf(item);
			_fakeContext[index] = updateStatusContent;
			return updateStatusContent;
		}
		
		public Task<FileIndexItem> UpdateItemAsync(FileIndexItem updateStatusContent)
		{
			return Task.FromResult(UpdateItem(updateStatusContent));
		}

		public Task<List<FileIndexItem>> UpdateItemAsync(List<FileIndexItem> updateStatusContentList)
		{
			foreach ( var item in updateStatusContentList )
			{
				UpdateItem(item);
			}
			return Task.FromResult(updateStatusContentList);
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
			return new RelativeObjects();
		}

		public List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList)
		{
			throw new System.NotImplementedException();
		}

		public bool AddCacheParentItem(string directoryName, List<FileIndexItem> items)
		{
			_fakeCachedContent ??= new List<FileIndexItem>();
			_fakeCachedContent.AddRange(items);
			return true;
		}

		public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
		{
			Console.WriteLine("CacheUpdateItem is called");
		}

		public void RemoveCacheItem(List<FileIndexItem> updateStatusContent)
		{
		}

		public void RemoveCacheItem(FileIndexItem updateStatusContent)
		{
		}

		public Tuple<bool, List<FileIndexItem>> CacheGetParentFolder(string subPath)
		{
			if ( _fakeCachedContent == null )
			{
				return new Tuple<bool, List<FileIndexItem>>(false, new List<FileIndexItem>());
			}
			
			var res =
				_fakeCachedContent.Where(p => p.ParentDirectory == subPath).ToList();
			return new Tuple<bool, List<FileIndexItem>>(res.Any(), res);
		}

		public async Task AddParentItemsAsync(string subPath)
		{
			var path = subPath == "/" || string.IsNullOrEmpty(subPath) ? "/" : PathHelper.RemoveLatestSlash(subPath);
			var pathListShouldExist = Breadcrumbs.BreadcrumbHelper(path).ToList();

			var toAddList = new List<FileIndexItem>();
			
			var indexItems = _fakeContext
				.Where(p => pathListShouldExist.Any(f => f == p.FilePath)).ToList();

			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach ( var pathShouldExist in pathListShouldExist )
			{
				if ( !indexItems.Select(p => p.FilePath).Contains(pathShouldExist) )
				{
					toAddList.Add(new FileIndexItem(pathShouldExist)
					{
						IsDirectory = true,
						AddToDatabase = DateTime.UtcNow,
						ColorClass = ColorClassParser.Color.None
					});
				}
			}

			await AddRangeAsync(toAddList);
		}

		public IQuery Clone(ApplicationDbContext applicationDbContext)
		{
			var query = (IQuery) MemberwiseClone();
			query.Invoke(applicationDbContext);
			return query;
		}

		public void Invoke(ApplicationDbContext applicationDbContext)
		{
		}

		public void SetGetObjectByFilePathCache(string filePath, FileIndexItem result,
			TimeSpan? cacheTime)
		{
		}

		public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}

		public bool IsCacheEnabled()
		{
			throw new System.NotImplementedException();
		}
	}
}
