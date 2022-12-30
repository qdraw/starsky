using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Services
{
	[Service(typeof(IThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class ThumbnailService : IThumbnailService
	{

		private readonly Thumbnail _thumbnail;

		public ThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger, AppSettings appSettings)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_thumbnail = new Thumbnail(iStorage, thumbnailStorage,logger,appSettings);
		}

		/// <summary>
		/// Create a thumbnail for a file or folder
		/// </summary>
		/// <param name="subPath">location on disk</param>
		/// <returns>object with status</returns>
		public Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
		{
			return _thumbnail.CreateThumbnailAsync(subPath);
		}

		/// <summary>
		/// Create for 1 image multiple thumbnails based on the default sizes
		/// </summary>
		/// <param name="subPath">path on disk (subPath) based</param>
		/// <param name="fileHash">output name</param>
		/// <returns>true if success</returns>
		public Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string subPath, string fileHash)
		{
			return _thumbnail.CreateThumbAsync(subPath, fileHash);
		}
	}
}
