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

				using ( var stream = await new Thumbnail(_iStorage,
					_thumbnailStorage,_logger).ResizeThumbnailFromSourceImage(
					item.FilePath, 4, null, true,
					ExtensionRolesHelper.ImageFormat.png) )
				{
					base64ImageArray[i] = "data:image/png;base64," + Base64Helper.ToBase64(stream);
					stream.Close();
				}

			}
			return base64ImageArray;
		}
	}
}
