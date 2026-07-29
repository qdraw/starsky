using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers;

public class ToBase64DataUriList(IThumbnailService thumbnailService)
{
	[SuppressMessage("Usage", "S3966: Resource 'memoryStream' has " +
	                          "already been disposed explicitly or through a using statement implicitly. " +
	                          "Remove the redundant disposal.")]
	public async Task<string[]> Create(List<FileIndexItem> fileIndexList)
	{
		var base64ImageArray = new string[fileIndexList.Count];
		for ( var i = 0; i < fileIndexList.Count; i++ )
		{
			var item = fileIndexList[i];
			const ThumbnailImageFormat format = ThumbnailImageFormat.webp;

			var (stream, status) = await thumbnailService.GenerateThumbnail(item.FilePath!,
				item.FileHash!,
				format,
				ThumbnailSize.TinyIcon);

			if ( !status.Success || stream == null )
			{
				// blank 1px x 1px WebP image (smaller & consistent across platforms)
				base64ImageArray[i] =
					"data:image/webp;base64,UklGRiYAAABXRUJQVlA4IBIAAAAwAQCdASoBAAEAQAcJaQCdLoAA3AA=";
				// no need to dispose here
				continue;
			}

			base64ImageArray[i] =
				$"data:image/{format};base64," + Base64Helper.ToBase64(stream);
			await stream.DisposeAsync();
		}

		return base64ImageArray;
	}
}
