using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Interfaces;

namespace starsky.foundation.thumbnailmeta.Services;

[Service(typeof(IMetaUpdateStatusThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MetaUpdateStatusThumbnailService : IMetaUpdateStatusThumbnailService
{
	private readonly FileHash _fileHashStorage;
	private readonly IThumbnailQuery _thumbnailQuery;

	public MetaUpdateStatusThumbnailService(IThumbnailQuery thumbnailQuery,
		ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_thumbnailQuery = thumbnailQuery;
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_fileHashStorage = new FileHash(storage, logger);
	}

	/// <summary>
	/// </summary>
	/// <param name="statusResultsWithSubPaths">fail/pass, string=subPath, string?2= error reason</param>
	public async Task UpdateStatusThumbnail(
		List<(bool, bool, string, string?)> statusResultsWithSubPaths)
	{
		var statusResultsWithFileHashes = new List<ThumbnailResultDataTransferModel>();
		foreach ( var (status, rightType, subPath, reason) in statusResultsWithSubPaths )
		{
			if ( !rightType )
			{
				continue;
			}

			var fileHash = ( await _fileHashStorage.GetHashCodeAsync(subPath) ).Key;
			statusResultsWithFileHashes.Add(new ThumbnailResultDataTransferModel(fileHash, status)
			{
				Reasons = reason
			});
		}

		await _thumbnailQuery.AddThumbnailRangeAsync(statusResultsWithFileHashes);
	}
}
