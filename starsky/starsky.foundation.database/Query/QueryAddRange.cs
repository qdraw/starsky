using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
			try
			{
				await _context.FileIndex.AddRangeAsync(fileIndexItemList);
				await _context.SaveChangesAsync();
			}
			catch (ObjectDisposedException)
			{
				var context = new InjectServiceScope(null, _scopeFactory).Context();
				await context.FileIndex.AddRangeAsync(fileIndexItemList);
				await context.SaveChangesAsync();
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
				var context = new InjectServiceScope(null, _scopeFactory).Context();
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
