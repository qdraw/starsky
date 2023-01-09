using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;

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
		
		var (newThumbnailItems, alreadyExistingThumbnailItems) = await CheckForDuplicates(
			dbContext, updateThumbnailNewItemsList);
		
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

	/// <summary>
	/// Check for Duplicates in the database
	/// </summary>
	/// <param name="context"></param>
	/// <param name="updateThumbnailNewItemsList"></param>
	/// <returns></returns>
	internal static async Task<(List<ThumbnailItem> newThumbnailItems,
		List<ThumbnailItem> alreadyExistingThumbnailItems)> 
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

		foreach ( var item in dbThumbnailItems )
		{
			var indexOfAlreadyExists = alreadyExistingThumbnailItems.FindIndex(p => p.FileHash == item.FileHash);
			if ( indexOfAlreadyExists == -1 ) continue;
			context.Attach(item).State = EntityState.Detached;

			alreadyExistingThumbnailItems[indexOfAlreadyExists].TinyMeta ??= item.TinyMeta;
			alreadyExistingThumbnailItems[indexOfAlreadyExists].Large ??= item.Large;
			alreadyExistingThumbnailItems[indexOfAlreadyExists].Small ??= item.Small;
			alreadyExistingThumbnailItems[indexOfAlreadyExists].ExtraLarge ??= item.ExtraLarge;
			alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons ??= item.Reasons;

			if ( ! alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons!.Contains(item.Reasons!) )
			{
				alreadyExistingThumbnailItems[indexOfAlreadyExists].Reasons += "," + item.Reasons;
			}
		}
		
		return ( newThumbnailItems, alreadyExistingThumbnailItems );
	}

}


