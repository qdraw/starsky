using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;

namespace starsky.foundation.database.Thumbnails;

[Service(typeof(IThumbnailQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailQuery : IThumbnailQuery
{
	private readonly ApplicationDbContext _context;
	private readonly IServiceScopeFactory? _scopeFactory;

	public ThumbnailQuery(ApplicationDbContext context, IServiceScopeFactory? scopeFactory)
	{
		_context = context;
		_scopeFactory = scopeFactory;
	}
	
	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		if ( thumbnailItems.Any(p => p.FileHash == null ) )
		{
			throw new ArgumentNullException(nameof(thumbnailItems));
		}
		return AddThumbnailRangeInternalRetryDisposedAsync(thumbnailItems);
	}

	private async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalRetryDisposedAsync(List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		try
		{
			return await AddThumbnailRangeInternalAsync(_context, thumbnailItems);
		}
		// InvalidOperationException can also be disposed
		catch (InvalidOperationException)
		{
			if ( _scopeFactory == null ) throw;
			return await AddThumbnailRangeInternalAsync(new InjectServiceScope(_scopeFactory).Context(), thumbnailItems);
		}
	}

	private static async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalAsync(
		ApplicationDbContext dbContext, 
		List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		if ( !thumbnailItems.Any() )
		{
			return new List<ThumbnailItem>();
		}
		
		var updateThumbnailNewItemsList = new List<ThumbnailItem>();
		foreach ( var item in thumbnailItems
			         .Where(p => p.FileHash != null).DistinctBy(p => p.FileHash) )
		{
			updateThumbnailNewItemsList.Add(new ThumbnailItem(item.FileHash!,item.TinyMeta, item.Small, item.Large, item.ExtraLarge, item.Reasons));
		}
		
		var (newThumbnailItems, 
				alreadyExistingThumbnailItems, 
				equalThumbnailItems) = 
			await CheckForDuplicates(dbContext, updateThumbnailNewItemsList);
		
		if ( newThumbnailItems.Any() )
		{
			await dbContext.Thumbnails.AddRangeAsync(newThumbnailItems);
			await dbContext.SaveChangesAsync();
		}
		
		if ( alreadyExistingThumbnailItems.Any() )
		{
			dbContext.Thumbnails.UpdateRange(alreadyExistingThumbnailItems);
			// not optimized for bulk operations yet
			await dbContext.SaveChangesAsync();
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

	public async Task<List<ThumbnailItem>> Get(string? fileHash = null)
	{
		return fileHash == null ? await _context
			.Thumbnails.ToListAsync() :  await _context
			.Thumbnails.Where(p => p.FileHash == fileHash)
			.ToListAsync();
	}

	public async Task RemoveThumbnails(List<string> deletedFileHashes)
	{
		if ( !deletedFileHashes.Any() ) return;
		foreach ( var fileNamesInChunk in deletedFileHashes.ChunkyEnumerable(100) )
		{
			var thumbnailItems = await _context.Thumbnails.Where(p => fileNamesInChunk.Contains(p.FileHash)).ToListAsync();
			_context.Thumbnails.RemoveRange(thumbnailItems);
			await _context.SaveChangesAsync();
		}
	}

	public async Task<bool> RenameAsync(string beforeFileHash, string newFileHash)
	{
		var beforeOrNewItems = await _context.Thumbnails.Where(p =>
			p.FileHash == beforeFileHash || p.FileHash == newFileHash).ToListAsync();
		
		var beforeItem = beforeOrNewItems.FirstOrDefault(p => p.FileHash == beforeFileHash);
		var newItem = beforeOrNewItems.FirstOrDefault(p => p.FileHash == newFileHash);

		if ( beforeItem == null) return false;

		_context.Thumbnails.Remove(beforeItem);

		if ( newItem != null )
		{
			_context.Thumbnails.Remove(newItem);
		}
		
		await _context.Thumbnails.AddRangeAsync(new ThumbnailItem(newFileHash, 
			beforeItem.TinyMeta, beforeItem.Small, beforeItem.Large, beforeItem.ExtraLarge, beforeItem.Reasons));
		
		await _context.SaveChangesAsync();
		
		return true;
	}

	/// <summary>
	/// Check for Duplicates in the database
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
				
		var newThumbnailItems = nonNullItems.Where(p => !alreadyExistingThumbnails.
			Contains(p!.FileHash)).Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();
		
		var alreadyExistingThumbnailItems = nonNullItems
			.Where(p => alreadyExistingThumbnails.Contains(p!.FileHash))
			.Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();

		var updateThumbnailItems = new List<ThumbnailItem>();
		var equalThumbnailItems = new List<ThumbnailItem>();
		
		// merge two items together
		foreach ( var item in dbThumbnailItems )
		{
			var indexOfAlreadyExists = alreadyExistingThumbnailItems.FindIndex(p => p.FileHash == item.FileHash);
			if ( indexOfAlreadyExists == -1 ) continue;
			var alreadyExists = alreadyExistingThumbnailItems[indexOfAlreadyExists];
			context.Attach(item).State = EntityState.Detached;

			alreadyExists.TinyMeta ??= item.TinyMeta;
			alreadyExists.Large ??= item.Large;
			alreadyExists.Small ??= item.Small;
			alreadyExists.ExtraLarge ??= item.ExtraLarge;
			alreadyExists.Reasons ??= item.Reasons;

			if ( !alreadyExists.Reasons!.Contains(item.Reasons!) )
			{
				var reasons = new StringBuilder(alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons);
				reasons.Append($",{item.Reasons}");
				alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons = reasons.ToString();
			}
			
			if ( item.TinyMeta == alreadyExists.TinyMeta  && 
			     item.Large == alreadyExists.Large  &&
			     item.Small == alreadyExists.Small &&
			     item.ExtraLarge == alreadyExists.ExtraLarge)
			{
				equalThumbnailItems.Add(alreadyExists);
				continue;
			}
			updateThumbnailItems.Add(alreadyExists);
		}
		
		return ( newThumbnailItems, updateThumbnailItems, equalThumbnailItems );
	}

}


