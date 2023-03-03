using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
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
		public virtual async Task<List<FileIndexItem>> AddRangeAsync(
			List<FileIndexItem> fileIndexItemList)
		{
			if ( !fileIndexItemList.Any() ) return new List<FileIndexItem>();

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
