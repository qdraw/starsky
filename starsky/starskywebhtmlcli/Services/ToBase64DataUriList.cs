using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskywebhtmlcli.Services
{
	public class ToBase64DataUriList
	{
		private IStorage _iStorage;

		public ToBase64DataUriList(IStorage iStorage)
		{
			_iStorage = iStorage;
		}
		
		public string[] Create(List<FileIndexItem> fileIndexList)
		{
			var base64ImageArray = new string[fileIndexList.Count];
			for (var i = 0; i<fileIndexList.Count; i++)
			{
				var item = fileIndexList[i];

				using ( var stream = new Thumbnail(_iStorage, null,null).ResizeThumbnail(item.FilePath, 4, 0, 0, true,
					ExtensionRolesHelper.ImageFormat.png) )
				{
					base64ImageArray[i] = "data:image/png;base64," + Base64Helper.ToBase64(stream);
				}

			}
			return base64ImageArray;
		}
	}
}
