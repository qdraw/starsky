#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

namespace starskytest.FakeMocks;

public class FakeIThumbnailQuery : IThumbnailQuery
{
	private readonly List<ThumbnailItem> _content = new List<ThumbnailItem>();

	public FakeIThumbnailQuery(List<ThumbnailItem>? items = null)
	{
		if ( items != null )
		{
			_content = items;
		}
	}

	public FakeIThumbnailQuery( ApplicationDbContext _, IServiceScope _2)
	{
		// should bind to the context
	}

	public Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(List<ThumbnailResultDataTransferModel> thumbnailItems)
	{
		var results = new List<ThumbnailItem?>();
		foreach ( var thumbnailItem in thumbnailItems )
		{
			var index = _content.FindIndex(p => p.FileHash == thumbnailItem.FileHash);
			if ( index == -1 )
			{
				var item = new ThumbnailItem()
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
		return Task.FromResult(fileHash == null ? _content : _content.Where(p => p.FileHash == fileHash).ToList());
	}

	public Task RemoveThumbnails(List<string> deletedFileHashes)
	{
		_content.RemoveAll(p => deletedFileHashes.Contains(p.FileHash));
		return Task.CompletedTask;
	}
}
