using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIThumbnailQuery : IThumbnailQuery
{
	private bool _isRunningJob;
	private readonly List<ThumbnailItem> _content = new();

	public FakeIThumbnailQuery(List<ThumbnailItem>? items = null, bool isRunningJob = false)
	{
		_isRunningJob = isRunningJob;
		if ( items != null )
		{
			_content = items;
		}
	}

	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	public FakeIThumbnailQuery(ApplicationDbContext _, IServiceScope _2, IWebLogger _3)
	{
		// should bind to the context
	}

	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(
		List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		var results = new List<ThumbnailItem?>();
		foreach ( var thumbnailItem in thumbnailItems )
		{
			var index = _content.FindIndex(p => p.FileHash == thumbnailItem.FileHash);
			if ( index == -1 )
			{
				var item = new ThumbnailItem
				{
					FileHash = thumbnailItem.FileHash!,
					Large = thumbnailItem.Large,
					ExtraLarge = thumbnailItem.ExtraLarge,
					Reasons = thumbnailItem.Reasons,
					Small = thumbnailItem.Small,
					TinyMeta = thumbnailItem.TinyMeta
				};
				_content.Add(item);
				results.Add(item);
				continue;
			}

			if ( thumbnailItem.Large != null )
			{
				_content[index].Large = thumbnailItem.Large;
			}

			if ( thumbnailItem.ExtraLarge != null )
			{
				_content[index].ExtraLarge = thumbnailItem.ExtraLarge;
			}

			if ( thumbnailItem.Reasons != null )
			{
				_content[index].Reasons = thumbnailItem.Reasons;
			}

			if ( thumbnailItem.Small != null )
			{
				_content[index].Small = thumbnailItem.Small;
			}

			if ( thumbnailItem.TinyMeta != null )
			{
				_content[index].TinyMeta = thumbnailItem.TinyMeta;
			}

			results.Add(_content[index]);
		}

		return Task.FromResult(results)!;
	}

	public Task<List<ThumbnailItem>> Get(string? fileHash = null)
	{
		return Task.FromResult(fileHash == null
			? _content
			: _content.Where(p => p.FileHash == fileHash).ToList());
	}

	public Task RemoveThumbnailsAsync(List<string> deletedFileHashes)
	{
		_content.RemoveAll(p => deletedFileHashes.Contains(p.FileHash));
		return Task.CompletedTask;
	}

	public Task<bool> RenameAsync(string beforeFileHash, string newFileHash)
	{
		var index = _content.FindIndex(p => p.FileHash == beforeFileHash);
		if ( index == -1 )
		{
			return Task.FromResult(false);
		}

		_content[index].FileHash = newFileHash;
		return Task.FromResult(true);
	}

	public Task<List<ThumbnailItem>> GetMissingThumbnailsBatchAsync(int pageNumber, int pageSize)
	{
		return Task.FromResult(_content
			.Where(p => ( p.ExtraLarge == null
			              || p.Large == null || p.Small == null )
			            && !string.IsNullOrEmpty(p.FileHash))
			.OrderBy(t => t.FileHash) // Ensure a consistent ordering
			.Skip(pageNumber * pageSize)
			.Take(pageSize).ToList());
	}

	public Task<bool> UpdateAsync(ThumbnailItem item)
	{
		var index = _content.FindIndex(p => p.FileHash == item.FileHash);
		if ( index == -1 )
		{
			return Task.FromResult(false);
		}

		_content[index] = item;
		return Task.FromResult(true);
	}

	public bool IsRunningJob()
	{
		return _isRunningJob;
	}

	public bool SetRunningJob(bool value)
	{
		_isRunningJob = value;
		return _isRunningJob;
	}
}
