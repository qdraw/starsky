using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
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
		private readonly IWebLogger _webLogger;

		public ExifToolHostStorageService(ISelectorStorage selectorStorage, AppSettings appSettings,IWebLogger webLogger)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_exifTool = new ExifTool(iStorage, thumbnailStorage, appSettings, _webLogger);
			_webLogger = webLogger;
		}
		
		public async Task<KeyValuePair<bool, string>> WriteTagsAsync(string subPath, string command)
		{
			return await _exifTool.WriteTagsAsync(subPath,command);
		}

		public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
		{
			return await _exifTool.WriteTagsThumbnailAsync(fileHash,command);
		}
	}
}
