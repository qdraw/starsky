using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Import;

[Service(typeof(IImportQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImportQuery : IImportQuery
{
	private readonly IConsole _console;
	private readonly ApplicationDbContext? _dbContext;
	private readonly bool _isConnection;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory? _scopeFactory;

	/// <summary>
	///     Query Already imported Database
	///     inject a scope to:
	///     @see:
	///     https://docs.microsoft.com/nl-nl/ef/core/miscellaneous/configuring-dbcontext#avoiding-dbcontext-threading-issues
	/// </summary>
	/// <param name="scopeFactory">to avoid threading issues with DbContext</param>
	/// <param name="console">console output</param>
	/// <param name="logger"></param>
	/// <param name="dbContext"></param>
	public ImportQuery(IServiceScopeFactory? scopeFactory, IConsole console, IWebLogger logger,
		ApplicationDbContext? dbContext = null)
	{
		_scopeFactory = scopeFactory;

		_console = console;
		_logger = logger;
		_dbContext = dbContext;
		_isConnection = TestConnection();
	}

	/// <summary>
	///     Test if the database connection is there
	/// </summary>
	/// <returns>successful database connection</returns>
	public bool TestConnection()
	{
		if ( _isConnection )
		{
			return _isConnection;
		}

		if ( _scopeFactory == null )
		{
			// fallback to injected dbContext (if provided)
			return _dbContext!.TestConnection(_logger);
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return scope.Execute(context => context.TestConnection(_logger));
	}

	public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
	{
		if ( !_isConnection )
		{
			// When there is no mysql connection continue
			return false;
		}

		if ( _scopeFactory == null )
		{
			return await _dbContext!.ImportIndex.CountAsync(p => p.FileHash == fileHashCode) != 0;
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return await scope.ExecuteAsync(async context =>
			await context.ImportIndex.CountAsync(p => p.FileHash == fileHashCode) != 0
		);
	}

	/// <summary>
	///     Add a new item to the imported database
	/// </summary>
	/// <param name="updateStatusContent">import database item</param>
	/// <param name="writeConsole">add icon to console</param>
	/// <returns>fail or success</returns>
	public async Task<bool> AddAsync(ImportIndexItem updateStatusContent,
		bool writeConsole = true)
	{
		updateStatusContent.AddToDatabase = DateTime.UtcNow;

		if ( _scopeFactory == null )
		{
			await _dbContext!.ImportIndex.AddAsync(updateStatusContent);
			await _dbContext.SaveChangesAsync();
			if ( writeConsole )
			{
				_console.Write("⬆️");
			}

			return true;
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return await scope.ExecuteAsync(async context =>
		{
			await context.ImportIndex.AddAsync(updateStatusContent);
			await context.SaveChangesAsync();
			if ( writeConsole )
			{
				_console.Write("⬆️");
			}

			return true;
		});
	}

	/// <summary>
	///     Get imported items for today
	/// </summary>
	/// <returns>List of items</returns>
	public List<ImportIndexItem> History()
	{
		if ( _scopeFactory == null )
		{
			return _dbContext!.ImportIndex
				.Where(p => p.AddToDatabase >= DateTime.UtcNow.AddDays(-1)).ToList();
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return scope.Execute(context =>
			context.ImportIndex.Where(p => p.AddToDatabase >= DateTime.UtcNow.AddDays(-1)).ToList()
		);
		// for debug: p.AddToDatabase >= DateTime.UtcNow.AddDays(-2) && p.Id % 6 == 1
	}

	public async Task<List<ImportIndexItem>> AddRangeAsync(
		List<ImportIndexItem> importIndexItemList)
	{
		if ( _scopeFactory == null )
		{
			await _dbContext!.ImportIndex.AddRangeAsync(importIndexItemList);
			await _dbContext.SaveChangesAsync();
			_console.Write($"⬆️ {importIndexItemList.Count} "); // arrowUp
			return importIndexItemList;
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return await scope.ExecuteAsync(async context =>
		{
			await context.ImportIndex.AddRangeAsync(importIndexItemList);
			await context.SaveChangesAsync();
			_console.Write($"⬆️ {importIndexItemList.Count} "); // arrowUp
			return importIndexItemList;
		});
	}

	public async Task<ImportIndexItem> RemoveItemAsync(ImportIndexItem importIndexItem,
		int maxAttemptCount = 3)
	{
		try
		{
			if ( _dbContext != null )
			{
				await LocalRemoveQuery(_dbContext);
			}
			else
			{
				await LocalRemoveDefaultQuery();
			}
		}
		catch ( SqliteException )
		{
			// Files that are locked
			await LocalRemoveQueryRetry();
		}
		catch ( ObjectDisposedException )
		{
			await LocalRemoveDefaultQuery();
		}
		catch ( InvalidOperationException )
		{
			await LocalRemoveQueryRetry();
		}
		catch ( DbUpdateConcurrencyException exception )
		{
			_logger.LogInformation("Import [RemoveItemAsync] catch-ed " +
			                       $"DbUpdateConcurrencyException (retry) {exception.Message}");
			await LocalRemoveQueryRetry();
		}

		return importIndexItem;

		async Task<bool> LocalRemoveDefaultQuery()
		{
			if ( _scopeFactory == null )
			{
				return false;
			}
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(async context1 =>
				await LocalRemoveQuery(context1)
			);
		}

		async Task<bool> LocalRemoveQuery(ApplicationDbContext context)
		{
			// Detach first https://stackoverflow.com/a/42475617
			var local = context.Set<ImportIndexItem>()
				.Local
				.FirstOrDefault(entry => entry.Id.Equals(importIndexItem.Id));
			if ( local != null )
			{
				context.Entry(local).State = EntityState.Detached;
			}

			// keep conditional marker for test
			context.ImportIndex?.Remove(importIndexItem);
			await context.SaveChangesAsync();
			return true;
		}

		async Task LocalRemoveQueryRetry()
		{
			try
			{
				await RetryHelper.DoAsync(LocalRemoveDefaultQuery, TimeSpan.FromSeconds(2),
					maxAttemptCount);
			}
			catch ( AggregateException exception )
			{
				_logger.LogInformation("Import [RemoveItemAsync] catch-ed " +
				                       $"AggregateException (ignored after retry) {exception.Message}");
			}
		}
	}


	public List<ImportIndexItem> AddRange(List<ImportIndexItem> importIndexItemList)
	{
		if ( _scopeFactory == null )
		{
			_dbContext?.ImportIndex.AddRange(importIndexItemList);
			_dbContext?.SaveChanges();
			_console.Write($"⬆️ {importIndexItemList.Count} ️"); // arrow up
			return importIndexItemList;
		}

		var scope = new InjectServiceScope(_scopeFactory);
		return scope.Execute(context =>
		{
			context.ImportIndex.AddRange(importIndexItemList);
			context.SaveChanges();
			_console.Write($"⬆️ {importIndexItemList.Count} ️"); // arrow up
			return importIndexItemList;
		});
	}
}
