using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query
{
	public partial class Query
	{
		/// <summary>
		/// Add a new item to the database
		/// </summary>
		/// <param name="fileIndexItemList"></param>
		/// <returns>items with id</returns>
		public virtual async Task<List<FileIndexItem>> AddRangeAsync(
			List<FileIndexItem> fileIndexItemList)
		{
			if ( fileIndexItemList.Count == 0 )
			{
				return new List<FileIndexItem>();
			}

			async Task LocalQuery(ApplicationDbContext context,
				IReadOnlyCollection<FileIndexItem> items)
			{
				await context.SaveChangesAsync();
				await context.FileIndex.AddRangeAsync(items);
				await context.SaveChangesAsync();
				foreach ( var item in items )
				{
					context.Attach(item).State = EntityState.Detached;
				}
			}
			
			async Task<bool> LocalRemoveDefaultQuery()
			{
				await LocalQuery(new InjectServiceScope(_scopeFactory).Context(), fileIndexItemList);
				return true;
			}

			try
			{
				await LocalQuery(_context, fileIndexItemList);
			}
			catch ( DbUpdateConcurrencyException concurrencyException )
			{
				SolveConcurrency.SolveConcurrencyExceptionLoop(
					concurrencyException.Entries);
				try
				{
					await _context.SaveChangesAsync();
				}
				catch ( DbUpdateConcurrencyException e )
				{
					if ( _appSettings.Verbose == true )
					{
						_context.ChangeTracker.DetectChanges();
						// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
						_logger?.LogDebug(_context.ChangeTracker.DebugView
							.LongView);
					}

					_logger?.LogError(e,
						"[AddRangeAsync] save failed after DbUpdateConcurrencyException");
				}
			}
			catch ( ObjectDisposedException )
			{
				await LocalQuery(
					new InjectServiceScope(_scopeFactory).Context(),
					fileIndexItemList);
			}
			catch ( Microsoft.Data.Sqlite.SqliteException )
			{
				// Files that are locked
				await RetryHelper.DoAsync(LocalRemoveDefaultQuery,
					TimeSpan.FromSeconds(2), 4);
			}

			fileIndexItemList = FormatOk(fileIndexItemList,
				FileIndexItem.ExifStatus.NotFoundNotInIndex);

			foreach ( var fileIndexItem in fileIndexItemList )
			{
				AddCacheItem(fileIndexItem);
			}

			return fileIndexItemList;
		}

	}
}
