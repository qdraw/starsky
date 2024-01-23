using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.sync.Helpers;

public sealed class AddParentList
{
	private readonly IStorage _subPathStorage;
	private readonly IQuery _query;

	public AddParentList(IStorage subPathStorage, IQuery query)
	{
		_subPathStorage = subPathStorage;
		_query = query;
	}
	
	public async Task<List<FileIndexItem>> AddParentItems(List<FileIndexItem> updatedDbItems)
	{
		// give parent folders back
		var addedParentItems = new List<FileIndexItem>();
		var okUpdatedDbItems = updatedDbItems.Where(p =>
			p.Status is FileIndexItem.ExifStatus.Ok
				or FileIndexItem.ExifStatus.OkAndSame
				or FileIndexItem.ExifStatus.Default).Select(p => p.ParentDirectory)
			.Distinct().Where(p => p != null).Cast<string>();
		
		foreach ( var subPath in okUpdatedDbItems
			         .Where(p => _subPathStorage.ExistFolder(p)))
		{
			var path = PathHelper.RemoveLatestSlash(subPath) + "/test.jpg";
			if ( subPath == "/" ) path = "/";
			addedParentItems.AddRange(await _query.AddParentItemsAsync(path));
		}
		updatedDbItems.AddRange(addedParentItems);
		return updatedDbItems;
	}
}
