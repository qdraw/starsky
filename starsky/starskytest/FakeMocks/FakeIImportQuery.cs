using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
#pragma warning disable 1998

namespace starskytest.FakeMocks
{
	public class FakeIImportQuery : IImportQuery
	{
		private readonly List<string> _exist;
		private readonly bool _isConnection;

		public FakeIImportQuery(List<string> exist, bool isConnection = true)
		{
			_exist = exist;
			_isConnection = isConnection;
			if ( exist == null ) _exist = new List<string>();
		}

		public FakeIImportQuery()
		{
			_exist = new List<string>();
			_isConnection = true;
		}

		/// <summary>
		/// To fake auto inject content
		/// </summary>
		/// <param name="scopeFactory"></param>
		/// <param name="dbContext"></param>
		public FakeIImportQuery(IServiceScopeFactory scopeFactory, ApplicationDbContext dbContext = null)
		{
			_exist = new List<string>();
			_isConnection = true;
		}
		
		public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{
			return _exist.Contains(fileHashCode);
		}

		public bool TestConnection()
		{
			return _isConnection;
		}

		public async Task<bool> AddAsync(ImportIndexItem updateStatusContent, bool console = true)
		{
			_exist.Add(updateStatusContent.FileHash);
			return true;
		}

		public List<ImportIndexItem> History()
		{
			var newFakeList = new List<ImportIndexItem>();
			foreach ( var exist in _exist )
			{
				newFakeList.Add(new ImportIndexItem
				{
					Status = ImportStatus.Ok,
					FilePath = exist
				});
			}
			return newFakeList;
		}

		public async Task<List<ImportIndexItem>> AddRangeAsync(List<ImportIndexItem> importIndexItemList)
		{
			foreach ( var importIndexItem in importIndexItemList )
			{
				await AddAsync(importIndexItem);
			}
			return importIndexItemList;
		}

		public async Task<bool> RemoveAsync(string fileHash)
		{
			_exist.Remove(fileHash);
			return true;
		}
	}
}
