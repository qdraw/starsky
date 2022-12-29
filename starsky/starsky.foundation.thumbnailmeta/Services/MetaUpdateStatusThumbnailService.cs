using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.metathumbnail.Services;

[Service(typeof(IMetaUpdateStatusThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MetaUpdateStatusThumbnailService : IMetaUpdateStatusThumbnailService
{
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly FileHash _fileHashStorage;

	public MetaUpdateStatusThumbnailService(IThumbnailQuery thumbnailQuery, ISelectorStorage selectorStorage)
	{
		_thumbnailQuery = thumbnailQuery;
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_fileHashStorage = new FileHash(storage);
	}
	
	public async Task UpdateStatusThumbnail(List<(bool, string, string?)> statusResultsWithSubPaths)
	{
		var statusResultsWithFileHashes = new List<(bool, string, string?)>();
		foreach ( var (status, subPath, reason) in statusResultsWithSubPaths )
		{
			var fileHash = ( await _fileHashStorage.GetHashCodeAsync(subPath)).Key;
			statusResultsWithFileHashes.Add((status, fileHash,reason));
		}
		
		var okItems = statusResultsWithFileHashes.Where(p => p.Item1).Select(p => p.Item2);
		await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.TinyMeta, okItems, true);
		
		var failItems = statusResultsWithFileHashes.Where(p => !p.Item1);
		await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.TinyMeta, failItems.Select(p => p.Item2), 
			false);
	}
}
