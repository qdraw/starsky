using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	public partial class Query
	{
		/// <summary>
		/// Add a new item to the database
		/// </summary>
		/// <param name="fileIndexItemList"></param>
		/// <returns>items with id</returns>
		public virtual async Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList)
		{
			async Task LocalQuery(ApplicationDbContext context)
			{
				await context.FileIndex.AddRangeAsync(fileIndexItemList);
				await context.SaveChangesAsync();
			}
			
			try
			{
				await LocalQuery(_context);
			}
			catch (ObjectDisposedException)
			{
				await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
			}

			foreach ( var fileIndexItem in fileIndexItemList )
			{
				AddCacheItem(fileIndexItem);
			}

			return fileIndexItemList;
		}
		
		/// <summary>
		/// (Sync) Add a new item to the database
		/// </summary>
		/// <param name="fileIndexItemList"></param>
		/// <returns>items with id</returns>
		public List<FileIndexItem> AddRange(List<FileIndexItem> fileIndexItemList)
		{
			try
			{
				_context.FileIndex.AddRange(fileIndexItemList);
				_context.SaveChanges();
			}
			catch (ObjectDisposedException)
			{
				var context = new InjectServiceScope(_scopeFactory).Context();
				context.FileIndex.AddRange(fileIndexItemList);
				context.SaveChanges();
			}

			foreach ( var fileIndexItem in fileIndexItemList )
			{
				AddCacheItem(fileIndexItem);
			}

			return fileIndexItemList;
		}

	}
}
