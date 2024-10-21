using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Thumbnails;

[Service(typeof(IThumbnailQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailQuery : IThumbnailQuery
{
	private readonly ApplicationDbContext _context;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory? _scopeFactory;

	public ThumbnailQuery(ApplicationDbContext context, IServiceScopeFactory? scopeFactory,
		IWebLogger logger)
	{
		_context = context;
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(
		List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		if ( thumbnailItems.Exists(p => string.IsNullOrEmpty(p.FileHash)) )
		{
			throw new ArgumentNullException(nameof(thumbnailItems),
				"[AddThumbnailRangeAsync] FileHash is null or empty");
		}

		return AddThumbnailRangeInternalRetryDisposedAsync(thumbnailItems);
	}

	public async Task<List<ThumbnailItem>> Get(string? fileHash = null)
	{
		try
		{
			return await GetInternalAsync(_context, fileHash);
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException )
		{
			if ( _scopeFactory == null )
			{
				throw;
			}

			return await GetInternalAsync(new InjectServiceScope(_scopeFactory).Context(),
				fileHash);
		}
	}

	public async Task RemoveThumbnailsAsync(List<string> deletedFileHashes)
	{
		if ( deletedFileHashes.Count == 0 )
		{
			return;
		}

		try
		{
			await RemoveThumbnailsInternalAsync(_context, deletedFileHashes);
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException )
		{
			if ( _scopeFactory == null )
			{
				throw;
			}

			await RemoveThumbnailsInternalAsync(new InjectServiceScope(_scopeFactory).Context(),
				deletedFileHashes);
		}
	}

	public async Task<bool> RenameAsync(string beforeFileHash, string newFileHash)
	{
		try
		{
			return await RenameInternalAsync(_context, beforeFileHash, newFileHash);
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException )
		{
			if ( _scopeFactory == null )
			{
				throw;
			}

			return await RenameInternalAsync(new InjectServiceScope(_scopeFactory).Context(),
				beforeFileHash, newFileHash);
		}
		catch ( DbUpdateConcurrencyException concurrencyException )
		{
			_logger.LogInformation("[ThumbnailQuery] try to fix DbUpdateConcurrencyException",
				concurrencyException);
			SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch ( DbUpdateConcurrencyException e )
			{
				_logger.LogInformation(e,
					"[ThumbnailQuery] save failed after DbUpdateConcurrencyException");
				return false;
			}

			return true;
		}
	}

	public async Task<bool> UpdateAsync(ThumbnailItem item)
	{
		try
		{
			return await UpdateInternalAsync(_context, item);
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException )
		{
			if ( _scopeFactory == null )
			{
				throw;
			}

			return await UpdateInternalAsync(new InjectServiceScope(_scopeFactory).Context(), item);
		}
	}

	private async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalRetryDisposedAsync(
		List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		try
		{
			return await AddThumbnailRangeInternalAsync(_context, thumbnailItems);
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException )
		{
			if ( _scopeFactory == null )
			{
				throw;
			}

			return await AddThumbnailRangeInternalAsync(
				new InjectServiceScope(_scopeFactory).Context(), thumbnailItems);
		}
	}

	private async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalAsync(
		ApplicationDbContext dbContext,
		IReadOnlyCollection<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		if ( thumbnailItems.Count == 0 )
		{
			return new List<ThumbnailItem>();
		}

		var updateThumbnailNewItemsList = new List<ThumbnailItem>();
		foreach ( var item in thumbnailItems
			         .Where(p => p.FileHash != null).DistinctBy(p => p.FileHash) )
		{
			updateThumbnailNewItemsList.Add(new ThumbnailItem(item.FileHash!, item.TinyMeta,
				item.Small, item.Large, item.ExtraLarge, item.Reasons));
		}

		var (newThumbnailItems,
				alreadyExistingThumbnailItems,
				equalThumbnailItems) =
			await CheckForDuplicates(dbContext, updateThumbnailNewItemsList);

		if ( newThumbnailItems.Count != 0 )
		{
			await dbContext.Thumbnails.AddRangeAsync(newThumbnailItems);
			await SaveChangesDuplicate(dbContext);
		}

		if ( alreadyExistingThumbnailItems.Count != 0 )
		{
			dbContext.Thumbnails.UpdateRange(alreadyExistingThumbnailItems);
			// not optimized for bulk operations yet
			await dbContext.SaveChangesAsync();
			await SaveChangesDuplicate(dbContext);
		}

		var allResults = alreadyExistingThumbnailItems
			.Concat(newThumbnailItems)
			.Concat(equalThumbnailItems)
			.ToList();

		foreach ( var item in allResults )
		{
			dbContext.Attach(item).State = EntityState.Detached;
		}

		return allResults;
	}

	private async Task SaveChangesDuplicate(DbContext dbContext)
	{
		try
		{
			await dbContext.SaveChangesAsync();
		}
		catch ( Exception exception )
		{
			// Check if the inner exception is a MySqlException
			var mySqlException = exception as MySqlException;
			// Skip if Duplicate entry
			// MySqlConnector.MySqlException (0x80004005): Duplicate entry for key 'PRIMARY'
			// https://github.com/qdraw/starsky/issues/1248 https://github.com/qdraw/starsky/issues/1489
			if ( mySqlException is { ErrorCode: MySqlErrorCode.DuplicateKey }
			    or { ErrorCode: MySqlErrorCode.DuplicateKeyEntry } )
			{
				_logger.LogInformation(
					"[SaveChangesDuplicate] OK Duplicate entry error occurred: " +
					$"{mySqlException.Message}");
				return;
			}

			_logger.LogError($"[SaveChangesDuplicate] T:{exception.GetType()} " +
			                 $"M:{exception.Message} " +
			                 $"I: {exception.InnerException} " +
			                 $"ErrorCode: {mySqlException?.ErrorCode}");

			throw;
		}
	}

	private static async Task<List<ThumbnailItem>> GetInternalAsync(
		ApplicationDbContext context,
		string? fileHash = null)
	{
		return fileHash == null
			? await context
				.Thumbnails.ToListAsync()
			: await context
				.Thumbnails.Where(p => p.FileHash == fileHash)
				.ToListAsync();
	}

	internal static async Task<bool> RemoveThumbnailsInternalAsync(
		ApplicationDbContext context,
		IReadOnlyCollection<string> deletedFileHashes)
	{
		if ( deletedFileHashes.Count == 0 )
		{
			return false;
		}

		foreach ( var fileNamesInChunk in deletedFileHashes.ChunkyEnumerable(100) )
		{
			var thumbnailItems = await context.Thumbnails
				.Where(p => fileNamesInChunk.Contains(p.FileHash)).ToListAsync();
			context.Thumbnails.RemoveRange(thumbnailItems);
			await context.SaveChangesAsync();
		}

		return true;
	}

	private async Task<bool> RenameInternalAsync(ApplicationDbContext dbContext,
		string? beforeFileHash, string? newFileHash)
	{
		if ( beforeFileHash == null || newFileHash == null )
		{
			_logger.LogError($"[ThumbnailQuery] Null " +
			                 $"beforeFileHash={beforeFileHash}; or newFileHash={newFileHash}; is null");
			return false;
		}

		var beforeOrNewItems = await dbContext.Thumbnails.Where(p =>
			p.FileHash == beforeFileHash || p.FileHash == newFileHash).ToListAsync();

		var beforeItem = beforeOrNewItems.Find(p => p.FileHash == beforeFileHash);
		var newItem = beforeOrNewItems.Find(p => p.FileHash == newFileHash);

		if ( beforeItem == null )
		{
			return false;
		}

		dbContext.Thumbnails.Remove(beforeItem);

		if ( newItem != null )
		{
			dbContext.Thumbnails.Remove(newItem);
		}

		await dbContext.Thumbnails.AddRangeAsync(new ThumbnailItem(newFileHash,
			beforeItem.TinyMeta, beforeItem.Small, beforeItem.Large, beforeItem.ExtraLarge,
			beforeItem.Reasons));

		await dbContext.SaveChangesAsync();

		return true;
	}

	public async Task<List<ThumbnailItem>> GetMissingThumbnailsBatchAsync(int pageNumber,
		int pageSize)
	{
		return await _context.Thumbnails
			.Where(p => ( p.ExtraLarge == null
			              || p.Large == null || p.Small == null )
			            && !string.IsNullOrEmpty(p.FileHash))
			.OrderBy(t => t.FileHash) // Ensure a consistent ordering
			.Skip(pageNumber * pageSize)
			.Take(pageSize)
			.ToListAsync();
	}

	internal static async Task<bool> UpdateInternalAsync(ApplicationDbContext dbContext,
		ThumbnailItem item)
	{
		dbContext.Thumbnails.Update(item);
		await dbContext.SaveChangesAsync();
		return true;
	}

	/// <summary>
	///     Check for Duplicates in the database
	/// </summary>
	/// <param name="context"></param>
	/// <param name="updateThumbnailNewItemsList"></param>
	/// <returns></returns>
	internal static async Task<(List<ThumbnailItem> newThumbnailItems,
			List<ThumbnailItem> updateThumbnailItems, List<ThumbnailItem> equalThumbnailItems)>
		CheckForDuplicates(ApplicationDbContext context,
			IEnumerable<ThumbnailItem?> updateThumbnailNewItemsList)
	{
		var nonNullItems = updateThumbnailNewItemsList.Where(item => item != null &&
			item.FileHash != null!).Distinct().ToList();

		var dbThumbnailItems = await context.Thumbnails
			.Where(p => nonNullItems.Select(x => x!.FileHash)
				.Contains(p.FileHash)).ToListAsync();
		var alreadyExistingThumbnails = dbThumbnailItems.Select(p => p.FileHash).Distinct();

		var newThumbnailItems = nonNullItems
			.Where(p => !alreadyExistingThumbnails.Contains(p!.FileHash)).Cast<ThumbnailItem>()
			.DistinctBy(p => p.FileHash).ToList();

		var alreadyExistingThumbnailItems = nonNullItems
			.Where(p => alreadyExistingThumbnails.Contains(p!.FileHash))
			.Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();

		var updateThumbnailItems = new List<ThumbnailItem>();
		var equalThumbnailItems = new List<ThumbnailItem>();

		// merge two items together
		foreach ( var item in dbThumbnailItems )
		{
			var indexOfAlreadyExists =
				alreadyExistingThumbnailItems.FindIndex(p => p.FileHash == item.FileHash);
			if ( indexOfAlreadyExists == -1 )
			{
				continue;
			}

			var alreadyExists = alreadyExistingThumbnailItems[indexOfAlreadyExists];
			context.Attach(item).State = EntityState.Detached;

			alreadyExists.TinyMeta ??= item.TinyMeta;
			alreadyExists.Large ??= item.Large;
			alreadyExists.Small ??= item.Small;
			alreadyExists.ExtraLarge ??= item.ExtraLarge;
			alreadyExists.Reasons ??= item.Reasons;

			if ( !alreadyExists.Reasons!.Contains(item.Reasons!) )
			{
				var reasons =
					new StringBuilder(alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons);
				reasons.Append($",{item.Reasons}");
				alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons = reasons.ToString();
			}

			if ( item.TinyMeta == alreadyExists.TinyMeta &&
			     item.Large == alreadyExists.Large &&
			     item.Small == alreadyExists.Small &&
			     item.ExtraLarge == alreadyExists.ExtraLarge )
			{
				equalThumbnailItems.Add(alreadyExists);
				continue;
			}

			updateThumbnailItems.Add(alreadyExists);
		}

		return ( newThumbnailItems, updateThumbnailItems, equalThumbnailItems );
	}
}
