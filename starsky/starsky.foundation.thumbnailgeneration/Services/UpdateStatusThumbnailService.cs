using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Services;

[Service(typeof(IUpdateStatusThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class UpdateStatusThumbnailService : IUpdateStatusThumbnailService
{
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly FileHash _fileHashStorage;

	public UpdateStatusThumbnailService(IThumbnailQuery thumbnailQuery, ISelectorStorage selectorStorage)
	{
		_thumbnailQuery = thumbnailQuery;
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_fileHashStorage = new FileHash(storage);
	}
	
	public Task UpdateStatusThumbnail(List<string> statusResultsWithSubPaths)
	{
		return Task.CompletedTask;
		// var statusResultsWithFileHashes = new List<(bool, string, string?)>();
		// foreach ( var (status, subPath, reason) in statusResultsWithSubPaths )
		// {
		// 	var fileHash = ( await _fileHashStorage.GetHashCodeAsync(subPath)).Key;
		// 	statusResultsWithFileHashes.Add((status, fileHash,reason));
		// }
		//
		// var okItems = statusResultsWithFileHashes.Where(p => p.Item1).Select(p => p.Item2);
		// await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.TinyMeta, okItems, true);
		//
		// var failItems = statusResultsWithFileHashes.Where(p => !p.Item1);
		// await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.TinyMeta, failItems.Select(p => p.Item2), 
		// 	false);
	}
}
