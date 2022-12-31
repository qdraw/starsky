using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.database.Thumbnails;

[Service(typeof(IThumbnailQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailQuery : IThumbnailQuery
{
	private readonly ApplicationDbContext _context;

	public ThumbnailQuery(ApplicationDbContext context)
	{
		_context = context;
	}
	
	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(
		ThumbnailSize size, IEnumerable<string> fileHashes,
		bool? setStatus = null)
	{
		if ( fileHashes == null )
		{
			throw new ArgumentNullException(nameof(fileHashes));
		}
		return AddThumbnailRangeInternalAsync(size, fileHashes, setStatus);
	}
	
	private async Task<List<ThumbnailItem>?> AddThumbnailRangeInternalAsync(
		ThumbnailSize size, IEnumerable<string> fileHashes,
		bool? setStatus = null)
	{
		var newItems = fileHashes.Distinct().Select(fileHash => new ThumbnailItem(fileHash, size, setStatus)).ToList();

		var (newThumbnailItems, alreadyExistingThumbnailItems) = await CheckForDuplicates(
			_context, newItems);
		
		if ( newThumbnailItems.Any() )
		{
			await _context.Thumbnails.AddRangeAsync(newThumbnailItems);
		}

		foreach ( var thumbnailItem in alreadyExistingThumbnailItems )
		{
			thumbnailItem.Change(size, setStatus);
		}

		await _context.SaveChangesAsync();

		var allResults = alreadyExistingThumbnailItems
			.Concat(newThumbnailItems)
			.ToList();
		
		foreach ( var item in allResults )
		{
			try
			{
				_context.Attach(item).State = EntityState.Detached;
			}
			catch ( InvalidOperationException)
			{
				// do nothing
			}
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
		
		var alreadyExistingThumbnailItems = nonNullItems.
			Where(p => alreadyExistingThumbnails.Contains(p!.FileHash)).Cast<ThumbnailItem>().DistinctBy(p => p.FileHash).ToList();
		
		return ( newThumbnailItems, alreadyExistingThumbnailItems );
	}

}


