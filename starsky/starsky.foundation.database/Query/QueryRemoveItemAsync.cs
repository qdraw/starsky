using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query
{
	
	/// <summary>
	/// QueryRemoveItemAsync
	/// </summary>
	public partial class Query
	{
		/// <summary>
		/// Remove a new item from the database (NOT from the file system)
		/// </summary>
		/// <param name="updateStatusContent">the FileIndexItem with database data</param>
		/// <returns></returns>
		public async Task<FileIndexItem> RemoveItemAsync(FileIndexItem updateStatusContent)
		{
			async Task<bool> LocalRemoveDefaultQuery()
			{
				await LocalRemoveQuery(new InjectServiceScope(_scopeFactory).Context());
				return true;
			}
        
			async Task LocalRemoveQuery(ApplicationDbContext context)
			{
				// Detach first https://stackoverflow.com/a/42475617
				var local = context.Set<FileIndexItem>()
					.Local
					.FirstOrDefault(entry => entry.Id.Equals(updateStatusContent.Id));
				if (local != null)
				{
					context.Entry(local).State = EntityState.Detached;
				}
				
				// keep conditional marker for test
				context.FileIndex?.Remove(updateStatusContent);
				await context.SaveChangesAsync();
			}
        
			try
			{
				await LocalRemoveQuery(_context);
			}
			catch ( Microsoft.Data.Sqlite.SqliteException )
			{
				// Files that are locked
				await RetryHelper.DoAsync(LocalRemoveDefaultQuery,
					TimeSpan.FromSeconds(2), 4);
			}
			catch ( ObjectDisposedException )
			{
				await LocalRemoveDefaultQuery();
			}
			catch ( InvalidOperationException )
			{
				await LocalRemoveDefaultQuery();
			}
			catch ( DbUpdateConcurrencyException e)
			{
				_logger.LogInformation(e,"[RemoveItemAsync] catch-ed " +
				                         "DbUpdateConcurrencyException (do nothing)");
			}
        
			// remove parent directory cache
			RemoveCacheItem(updateStatusContent);
        
			// remove getFileHash Cache
			ResetItemByHash(updateStatusContent.FileHash);
			return updateStatusContent;
		}
			    
		/// <summary>
		/// Remove a new item from the database (NOT from the file system)
		/// </summary>
		/// <param name="updateStatusContentList">the FileIndexItem with database data</param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> RemoveItemAsync(List<FileIndexItem> updateStatusContentList)
		{
			async Task<bool> LocalRemoveDefaultQuery()
			{
				await LocalRemoveQuery(new InjectServiceScope(_scopeFactory).Context());
				return true;
			}
    
			async Task LocalRemoveQuery(ApplicationDbContext context)
			{
				// Detach first https://stackoverflow.com/a/42475617
				foreach ( var updateStatusContent in updateStatusContentList )
				{
					var local = context.Set<FileIndexItem>()
						.Local
						.FirstOrDefault(entry => entry.Id.Equals(updateStatusContent.Id));
					if (local != null)
					{
						context.Entry(local).State = EntityState.Detached;
					}
				}
				// keep conditional marker for test
				context.FileIndex?.RemoveRange(updateStatusContentList);
				await context.SaveChangesAsync();
			}
    
			try
			{
				await LocalRemoveQuery(_context);
			}
			catch ( Microsoft.Data.Sqlite.SqliteException )
			{
				// Files that are locked
				await RetryHelper.DoAsync(LocalRemoveDefaultQuery,
					TimeSpan.FromSeconds(2), 4);
			}
			catch ( ObjectDisposedException )
			{
				await LocalRemoveDefaultQuery();
			}
			catch ( InvalidOperationException )
			{
				await LocalRemoveDefaultQuery();
			}
			catch ( DbUpdateConcurrencyException e)
			{
				_logger.LogInformation(e,"[RemoveItemAsync:List] catch-ed " +
				                         "DbUpdateConcurrencyException (do nothing)");
			}
    
			// remove parent directory cache
			RemoveCacheItem(updateStatusContentList);
    
			// remove getFileHash Cache
			foreach ( var updateStatusContent in updateStatusContentList )
			{
				ResetItemByHash(updateStatusContent.FileHash);
			}
			return updateStatusContentList;
		}
	}
}
