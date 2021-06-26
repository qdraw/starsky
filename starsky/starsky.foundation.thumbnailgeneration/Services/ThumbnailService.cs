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

		/// <summary>
		/// Create an Thumbnail based on the default sizes
		/// </summary>
		/// <param name="subPath">path on disk (subPath) based</param>
		/// <returns>true if success</returns>
		public Task<List<(string, bool)>> CreateThumb(string subPath)
		{
			// Async method:
			return _thumbnail.CreateThumb(subPath);
		}

		/// <summary>
		/// Create an Thumbnail based on the default sizes
		/// </summary>
		/// <param name="subPath">path on disk (subPath) based</param>
		/// <param name="fileHash">output name</param>
		/// <returns>true if success</returns>
		public Task<bool> CreateThumb(string subPath, string fileHash)
		{
			// Async method:
			return _thumbnail.CreateThumb(subPath, fileHash);
		}
	}
}
