using System.Collections.Generic;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

public class ThumbnailSizesChecker(IStorage thumbnailStorage, AppSettings appSettings)
{
	public List<ThumbnailSize> ListThumbnailToBeCreated(List<ThumbnailSize> sizesToCheck,
		string fileHash)
	{
		// And create then a thumbnail from the extra large thumbnail
		// to the small thumbnail
		var thumbnailFromThumbnailUpdateList = new List<ThumbnailSize>();

		sizesToCheck.ForEach(AddFileNames);

		return thumbnailFromThumbnailUpdateList;

		void AddFileNames(ThumbnailSize size)
		{
			if ( !thumbnailStorage.ExistFile(
				    ThumbnailNameHelper.Combine(
					    fileHash, size, appSettings.ThumbnailImageFormat))
			   )
			{
				thumbnailFromThumbnailUpdateList.Add(size);
			}
		}
	}
}
