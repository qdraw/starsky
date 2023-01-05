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
using starsky.foundation.platform.Enums;

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
	
	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(
		List<ThumbnailSize> size, IReadOnlyCollection<string> fileHashes,
		bool? setStatus = null)
	{
		if ( fileHashes == null )
		{
			throw new ArgumentNullException(nameof(fileHashes));
		}
		return AddThumbnailRangeInternalRetryDisposedAsync(size, fileHashes, setStatus);
	}

	private async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalRetryDisposedAsync(List<ThumbnailSize> size,
			IReadOnlyCollection<string> fileHashes,
			bool? setStatus = null)
	{
		try
		{
			return await AddThumbnailRangeInternalAsync(_context, size,
				fileHashes, setStatus);
		}
		// InvalidOperationException can also be disposed
		catch (InvalidOperationException)
		{
			if ( _scopeFactory == null ) throw;
			return await AddThumbnailRangeInternalAsync(new InjectServiceScope(_scopeFactory).Context(), size,
				fileHashes, setStatus);
		}
	}

	private static async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalAsync(
		ApplicationDbContext dbContext, 
		List<ThumbnailSize> sizes, IEnumerable<string> fileHashes,
		bool? setStatus = null)
	{
		if ( !sizes.Any() )
		{
			return new List<ThumbnailItem>();
		}
		
		var newItems = fileHashes.Distinct().Select(fileHash => 
			new ThumbnailItem(fileHash, 
			sizes.FirstOrDefault(), setStatus)).ToList();

		foreach ( var thumbnailItem in newItems )
		{
			foreach ( var size in sizes )
			{
				thumbnailItem.Change(size, setStatus);
			}
		}
		
		var (newThumbnailItems, alreadyExistingThumbnailItems) = await CheckForDuplicates(
			dbContext, newItems);
		
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
	/// <param name="items"></param>
	/// <returns></returns>
	internal static async Task<(List<ThumbnailItem> newThumbnailItems,
		List<ThumbnailItem> alreadyExistingThumbnailItems)> 
		CheckForDuplicates(ApplicationDbContext context, 
			IEnumerable<ThumbnailItem?> items)
	{
		var nonNullItems = items.Where(item => item != null && 
		                                       item.FileHash != null!).Distinct().ToList();
		
		var alreadyExistingThumbnails = await context.Thumbnails
			.Where(p => nonNullItems.Select(x => x!.FileHash)
			.Contains(p.FileHash)).Select(p => p.FileHash).Distinct().ToListAsync();
				
		var newThumbnailItems = nonNullItems.Where(p => !alreadyExistingThumbnails.
			Contains(p!.FileHash)).Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();
		
		var alreadyExistingThumbnailItems = nonNullItems
			.Where(p => alreadyExistingThumbnails.Contains(p!.FileHash))
			.Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();
		
		return ( newThumbnailItems, alreadyExistingThumbnailItems );
	}

}


