#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

namespace starskytest.FakeMocks;

public class FakeIThumbnailQuery : IThumbnailQuery
{
	private readonly List<ThumbnailItem> _content = new List<ThumbnailItem>();

	public FakeIThumbnailQuery(List<ThumbnailItem> items = null)
	{
		if ( items != null )
		{
			_content = items;
		}
	}

	public FakeIThumbnailQuery( ApplicationDbContext _)
	{
		// should bind to the context
	}
	
	public Task<List<ThumbnailItem>> AddThumbnailRangeAsync(ThumbnailSize size, IEnumerable<string> fileHashes,
		bool? setStatus = null)
	{
		foreach ( var hash in fileHashes )
		{
			var index = _content.FindIndex(p => p.FileHash == hash);
			if ( index == -1 )
			{
				_content.Add(new ThumbnailItem(hash, size, setStatus));
				continue;
			}
			
			_content[index].Change(size, setStatus);
		}
		
		return Task.FromResult(_content);
	}

	public Task<List<ThumbnailItem>> Get(string? fileHash = null)
	{
		return Task.FromResult(fileHash == null ? _content : _content.Where(p => p.FileHash == fileHash).ToList());
	}
}
