using System.Collections.Generic;
using starsky.foundation.database.Models;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskywebhtmlcli.Services
{
	public class ToBase64DataUriList
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;

		public ToBase64DataUriList(IStorage iStorage, IStorage thumbnailStorage)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
		}
		
		public string[] Create(List<FileIndexItem> fileIndexList)
		{
			var base64ImageArray = new string[fileIndexList.Count];
			for (var i = 0; i<fileIndexList.Count; i++)
			{
				var item = fileIndexList[i];

				using ( var stream = new Thumbnail(_iStorage,_thumbnailStorage).ResizeThumbnail(item.FilePath, 4, null, true,
					ExtensionRolesHelper.ImageFormat.png) )
				{
					base64ImageArray[i] = "data:image/png;base64," + Base64Helper.ToBase64(stream);
				}

			}
			return base64ImageArray;
		}
	}
}
