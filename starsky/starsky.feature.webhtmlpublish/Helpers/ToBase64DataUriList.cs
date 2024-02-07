using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Helpers;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class ToBase64DataUriList
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;

		public ToBase64DataUriList(IStorage iStorage, IStorage thumbnailStorage, IWebLogger logger,
			AppSettings appSettings)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
			_logger = logger;
			_appSettings = appSettings;
		}

		[SuppressMessage("Usage", "S3966: Resource 'memoryStream' has " +
		                          "already been disposed explicitly or through a using statement implicitly. " +
		                          "Remove the redundant disposal.")]
		public async Task<string[]> Create(List<FileIndexItem> fileIndexList)
		{
			var base64ImageArray = new string[fileIndexList.Count];
			for ( var i = 0; i < fileIndexList.Count; i++ )
			{
				var item = fileIndexList[i];

				var service = new Thumbnail(_iStorage,
					_thumbnailStorage, _logger, _appSettings);

				var (memoryStream, status, _) = await service.ResizeThumbnailFromSourceImage(
					item.FilePath!, 4, null, true,
					ExtensionRolesHelper.ImageFormat.png);

				if ( !status )
				{
					// blank 1px x 1px image
					base64ImageArray[i] =
						"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAA" +
						"C1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
					// no need to dispose here
					continue;
				}

				base64ImageArray[i] =
					"data:image/png;base64," + Base64Helper.ToBase64(memoryStream!);
				await memoryStream!.DisposeAsync();
			}

			return base64ImageArray;
		}
	}
}
