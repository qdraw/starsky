using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Services
{
	[Service(typeof(IThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ThumbnailService : IThumbnailService
	{

		private readonly Thumbnail _thumbnail;

		public ThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_thumbnail = new Thumbnail(iStorage, thumbnailStorage,logger);
		}

		public Task<List<(string, bool)>> CreateThumb(string subPath)
		{
			return _thumbnail.CreateThumb(subPath);
		}

		public Task<bool> CreateThumb(string subPath, string fileHash)
		{
			return _thumbnail.CreateThumb(subPath, fileHash);
		}
	}
}
