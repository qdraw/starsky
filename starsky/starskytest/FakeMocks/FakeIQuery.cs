using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIQuery : IQuery
	{
		public FakeIQuery(List<FileIndexItem> fakeContext = null)
		{
			if ( fakeContext == null ) return;
			_fakeContext = fakeContext;
		}

		public FakeIQuery(ApplicationDbContext context, 
			IMemoryCache memoryCache = null, 
			AppSettings appSettings = null,
			IServiceScopeFactory scopeFactory = null)
		{
		}
		
		private List<FileIndexItem> _fakeContext = new List<FileIndexItem>();
		
		public List<FileIndexItem> GetAllFiles(string subPath)
		{
			return _fakeContext.Where(p => p.ParentDirectory == subPath && p.IsDirectory == false).ToList();
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

		public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", 
			List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<FileIndexItem> DisplayFileFolders(List<FileIndexItem> fileIndexItems, List<ColorClassParser.Color> 
				colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public DetailView SingleItem(string singleItemDbPath, List<ColorClassParser.Color> colorClassActiveList = null,
			bool enableCollections = true, bool hideDeleted = true)
		{
			if ( _fakeContext.All(p => p.FilePath != singleItemDbPath) )
			{
				return null;
			}

			var fileIndexItem = _fakeContext.FirstOrDefault(p => p.FilePath == singleItemDbPath);
			if ( fileIndexItem == null ) return null;
			fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			fileIndexItem.CollectionPaths = new List<string>{singleItemDbPath};
			return new DetailView {FileIndexItem = fileIndexItem,};
		}

		public DetailView SingleItem(List<FileIndexItem> fileIndexItemsList, string singleItemDbPath,
			List<ColorClassParser.Color> colorClassActiveList = null, bool enableCollections = true, bool hideDeleted = true)
		{
			throw new System.NotImplementedException();
		}

		public FileIndexItem GetObjectByFilePath(string filePath)
		{
			return _fakeContext.FirstOrDefault(p => p.FilePath == filePath);
		}

		public async Task<FileIndexItem> GetObjectByFilePathAsync(string filePath)
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
			return _fakeContext.Where(p => p.IsDirectory == true).ToList();
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
			_fakeContext = _fakeContext.Where(p => p.FilePath == updateStatusContent.FilePath)
				.Select(c => updateStatusContent).ToList();
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
			throw new System.NotImplementedException();
		}

		public List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList)
		{
			throw new System.NotImplementedException();
		}

		public void CacheUpdateItem(List<FileIndexItem> updateStatusContent)
		{
			Console.WriteLine("CacheUpdateItem is called");
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

		public bool IsCacheEnabled()
		{
			throw new System.NotImplementedException();
		}
	}
}
