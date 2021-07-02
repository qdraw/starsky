using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Helpers;
using starskycore.Helpers;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class ToBase64DataUriList
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;

		public ToBase64DataUriList(IStorage iStorage, IStorage thumbnailStorage, IWebLogger logger)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
			_logger = logger;
		}
		
		public async Task<string[]> Create(List<FileIndexItem> fileIndexList)
		{
			var base64ImageArray = new string[fileIndexList.Count];
			for (var i = 0; i<fileIndexList.Count; i++)
			{
				var item = fileIndexList[i];

				var (memoryStream, status, _) = ( await new Thumbnail(_iStorage,
					_thumbnailStorage, _logger).ResizeThumbnailFromSourceImage(
					item.FilePath, 4, null, true,
					ExtensionRolesHelper.ImageFormat.png));

				if ( !status )
				{
					// blank 1px x 1px image
					base64ImageArray[i] = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAA" +
					                      "C1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
					// no need to dispose here
					continue;
				}
				
				base64ImageArray[i] = "data:image/png;base64," + Base64Helper.ToBase64(memoryStream);
				memoryStream.Dispose();
			}

			return base64ImageArray;
		}
	}
}
