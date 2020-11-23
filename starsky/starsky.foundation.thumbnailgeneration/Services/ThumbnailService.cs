using starsky.foundation.injection;
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

		public ThumbnailService(ISelectorStorage selectorStorage)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_thumbnail = new Thumbnail(iStorage, thumbnailStorage);
		}

		public bool CreateThumb(string subPath)
		{
			return _thumbnail.CreateThumb(subPath);
		}

		public bool CreateThumb(string subPath, string fileHash)
		{
			return _thumbnail.CreateThumb(subPath, fileHash);
		}
	}
}
