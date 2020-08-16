using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.writemeta.Services
{
	[Service(typeof(IExifToolHostStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ExifToolHostStorageService : IExifToolHostStorage
	{
		private readonly IExifTool _exifTool;

		public ExifToolHostStorageService(ISelectorStorage selectorStorage, AppSettings appSettings)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_exifTool = new ExifTool(iStorage, thumbnailStorage, appSettings);

		}
		public async Task<bool> WriteTagsAsync(string subPath, string command)
		{
			return await _exifTool.WriteTagsAsync(subPath,command);
		}

		public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
		{
			return await _exifTool.WriteTagsThumbnailAsync(fileHash,command);
		}
	}
}
