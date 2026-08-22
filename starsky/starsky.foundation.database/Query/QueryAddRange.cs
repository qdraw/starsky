using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

/// <summary>
///     QueryAddRange
/// </summary>
public partial class Query
{
	/// <summary>
	///     Add a new item to the database
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

		try
		{
			await AddRangeInternalAsync(_context, fileIndexItemList);
		}
		catch ( DbUpdateConcurrencyException concurrencyException )
		{
			await HandleConcurrencyExceptionAsync(concurrencyException);
		}
		catch ( ObjectDisposedException )
		{
			await AddRangeWithNewScopeInternalAsync(fileIndexItemList);
		}
		catch ( SqliteException sqliteEx )
		{
			await HandleSqliteExceptionAsync(sqliteEx, fileIndexItemList);
		}
		catch ( DbUpdateException dbEx )
		{
			await HandleDbUpdateExceptionAsync(dbEx, fileIndexItemList);
		}

		fileIndexItemList = FormatOk(fileIndexItemList,
			FileIndexItem.ExifStatus.NotFoundNotInIndex);

		foreach ( var fileIndexItem in fileIndexItemList )
		{
			AddCacheItem(fileIndexItem);
		}

		return fileIndexItemList;
	}

	private static async Task AddRangeInternalAsync(ApplicationDbContext context,
		IReadOnlyCollection<FileIndexItem> items)
	{
		await context.FileIndex.AddRangeAsync(items);
		await context.SaveChangesAsync();
		foreach ( var item in items )
		{
			context.Attach(item).State = EntityState.Detached;
		}
	}

	private async Task<bool> FilterDuplicatesAndRetryAsync(List<FileIndexItem> items)
	{
		var scope = new InjectServiceScope(_scopeFactory);
		return await scope.ExecuteAsync(async context =>
		{
			var existingPaths = await context.FileIndex
				.Where(f => items.Select(x => x.FilePath).Contains(f.FilePath))
				.Select(f => f.FilePath)
				.ToListAsync();

			var itemsToAdd = items
				.Where(item => !existingPaths.Contains(item.FilePath))
				.ToList();

			if ( itemsToAdd.Count == 0 )
			{
				_logger.LogInformation(
					"[AddRangeAsync] All items already exist in database, skipping insert");
				return true;
			}

			await AddRangeInternalAsync(context, itemsToAdd);
			return true;
		});
	}

	private async Task<bool> AddRangeWithNewScopeInternalAsync(List<FileIndexItem> items)
	{
		var scope = new InjectServiceScope(_scopeFactory);
		return await scope.ExecuteAsync(async context =>
		{
			await AddRangeInternalAsync(context, items);
			return true;
		});
	}

	private async Task HandleConcurrencyExceptionAsync(
		DbUpdateConcurrencyException concurrencyException)
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
				_logger.LogDebug(_context.ChangeTracker.DebugView.LongView);
			}

			_logger.LogError(e,
				"[AddRangeAsync] save failed after DbUpdateConcurrencyException");
		}
	}

	private async Task HandleSqliteExceptionAsync(SqliteException sqliteEx,
		List<FileIndexItem> items)
	{
		if ( sqliteEx.SqliteErrorCode == 19 )
		{
			_logger.LogInformation(
				"[AddRangeAsync] UNIQUE constraint violation detected, filtering duplicates");
			await RetryHelper.DoAsync(() => FilterDuplicatesAndRetryAsync(items),
				TimeSpan.FromSeconds(2), 4);
		}
		else
		{
			await RetryHelper.DoAsync(() => AddRangeWithNewScopeInternalAsync(items),
				TimeSpan.FromSeconds(2), 4);
		}
	}

	private async Task HandleDbUpdateExceptionAsync(DbUpdateException dbEx,
		List<FileIndexItem> items)
	{
		if ( IsUniqueConstraintViolation(dbEx) )
		{
			_logger.LogInformation(
				"[AddRangeAsync] UNIQUE constraint violation in DbUpdateException, filtering duplicates");
			await RetryHelper.DoAsync(() => FilterDuplicatesAndRetryAsync(items),
				TimeSpan.FromSeconds(2), 4);
		}
		else
		{
			await RetryHelper.DoAsync(() => AddRangeWithNewScopeInternalAsync(items),
				TimeSpan.FromSeconds(2), 4);
		}
	}

	private static bool IsUniqueConstraintViolation(DbUpdateException dbEx)
	{
		return dbEx.InnerException is SqliteException { SqliteErrorCode: 19 };
	}
}
