#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.writemeta.Services
{
	[Service(typeof(IExifTool), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class ExifToolService : IExifTool
	{
		private readonly ExifTool _exifTool;

		public ExifToolService(ISelectorStorage selectorStorage, AppSettings appSettings, IWebLogger logger)
		{
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_exifTool = new ExifTool(iStorage, thumbnailStorage, appSettings,logger);

		}
		public async Task<bool> WriteTagsAsync(string subPath, string command)
		{
			return await _exifTool.WriteTagsAsync(subPath,command);
		}
		
		public async Task<KeyValuePair<bool, string>> WriteTagsAndRenameThumbnailAsync(string subPath, 
			string? beforeFileHash, string command, CancellationToken cancellationToken = default)
		{
			return await _exifTool.WriteTagsAndRenameThumbnailAsync(subPath,beforeFileHash,command, cancellationToken);
		}

		public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
		{
			return await _exifTool.WriteTagsThumbnailAsync(fileHash,command);
		}
	}
}
