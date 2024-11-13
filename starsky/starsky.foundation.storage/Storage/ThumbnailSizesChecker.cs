using System.Collections.Generic;
using starsky.foundation.platform.Enums;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

public class ThumbnailSizesChecker
{
	private readonly IStorage _thumbnailStorage;

	public ThumbnailSizesChecker(IStorage thumbnailStorage)
	{
		_thumbnailStorage = thumbnailStorage;
	}

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
			if ( !_thumbnailStorage.ExistFile(
				    ThumbnailNameHelper.Combine(
					    fileHash, size))
			   )
			{
				thumbnailFromThumbnailUpdateList.Add(size);
			}
		}
	}
}
