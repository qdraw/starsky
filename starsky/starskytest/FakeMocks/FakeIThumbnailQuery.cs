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
		return Task.FromResult(_content);
	}

	public Task<List<ThumbnailItem>> Get(string fileHash)
	{
		return Task.FromResult(
			_content.Where(p => p.FileHash == fileHash).ToList()
			);
	}
}
