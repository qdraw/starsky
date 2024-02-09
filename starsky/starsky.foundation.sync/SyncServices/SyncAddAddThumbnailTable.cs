using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices;

[Service(typeof(ISyncAddThumbnailTable), InjectionLifetime = InjectionLifetime.Scoped)]
public class SyncAddAddThumbnailTable : ISyncAddThumbnailTable
{
	private readonly IThumbnailQuery _thumbnailQuery;

	public SyncAddAddThumbnailTable(IThumbnailQuery thumbnailQuery)
	{
		_thumbnailQuery = thumbnailQuery;
	}

	public async Task<List<FileIndexItem>> SyncThumbnailTableAsync(List<FileIndexItem> fileIndexItems)
	{
		var addObjects = fileIndexItems
			.Where(p => p.Status == FileIndexItem.ExifStatus.Ok &&
						p.ImageFormat != ExtensionRolesHelper.ImageFormat.xmp &&
						p.ImageFormat != ExtensionRolesHelper.ImageFormat.meta_json &&
						!string.IsNullOrEmpty(p.FileHash) && p.IsDirectory == false)
			.DistinctBy(p => p.FileHash)
			.Select(p => p.FileHash)
			.Select(fileHash => new ThumbnailResultDataTransferModel(fileHash!)).ToList();

		await _thumbnailQuery.AddThumbnailRangeAsync(addObjects);

		return fileIndexItems;
	}
}


