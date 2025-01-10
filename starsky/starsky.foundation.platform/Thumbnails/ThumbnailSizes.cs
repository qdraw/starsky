using System.Collections.Generic;

namespace starsky.foundation.platform.Thumbnails;

public static class ThumbnailSizes
{
	public static List<ThumbnailSize> GetSizes(bool skipExtraLarge)
	{
		var sizesList = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		if ( !skipExtraLarge )
		{
			sizesList.Add(ThumbnailSize.ExtraLarge);
		}

		return sizesList;
	}

	public static List<ThumbnailSize> GetLargeToSmallSizes(bool skipExtraLarge)
	{
		var sizesList = new List<ThumbnailSize>();

		if ( !skipExtraLarge )
		{
			sizesList.Add(ThumbnailSize.ExtraLarge);
		}

		sizesList.AddRange(new List<ThumbnailSize> { ThumbnailSize.Large, ThumbnailSize.Small });
		return sizesList;
	}
}
