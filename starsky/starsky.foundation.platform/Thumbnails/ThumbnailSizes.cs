using System.Collections.Generic;

namespace starsky.foundation.platform.Thumbnails;

public static class ThumbnailSizes
{
	public static List<ThumbnailSize> GetLargeToSmallSizes(
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		var sizesList = new List<ThumbnailSize>();

		if ( type == ThumbnailGenerationType.All )
		{
			sizesList.Add(ThumbnailSize.ExtraLarge);
		}

		if ( type is ThumbnailGenerationType.All or ThumbnailGenerationType.SkipExtraLarge )
		{
			sizesList.Add(ThumbnailSize.Large);
		}

		sizesList.AddRange(new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.TinyMeta });
		return sizesList;
	}
}
