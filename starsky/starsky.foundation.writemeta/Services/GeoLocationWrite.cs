using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starsky.foundation.writemeta.Services
{
	[Service(typeof(IGeoLocationWrite), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class GeoLocationWrite : IGeoLocationWrite
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly ExifToolCmdHelper _exifToolCmdHelper;

		public GeoLocationWrite(AppSettings appSettings, IExifTool exifTool, 
			ISelectorStorage selectorStorage, IConsole console, IWebLogger logger, IThumbnailQuery thumbnailQuery)
		{
			_appSettings = appSettings;
			var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_console = console;
			_exifToolCmdHelper = new ExifToolCmdHelper(exifTool, 
				iStorage, 
				thumbnailStorage, 
				new ReadMeta(iStorage, _appSettings, null, logger),thumbnailQuery);
		}

		/// <summary>
		/// Write to ExifTool by list
		/// </summary>
		/// <param name="metaFilesInDirectory">list of files with data</param>
		/// <param name="syncLocationNames">Write city, state and country to exifTool (false > no)</param>
		public async Task LoopFolderAsync(List<FileIndexItem> metaFilesInDirectory,
			bool syncLocationNames)
		{
			foreach ( var metaFileItem in metaFilesInDirectory.Where(metaFileItem => 
				         ExtensionRolesHelper.IsExtensionExifToolSupported(metaFileItem.FileName)) )
			{
				if ( _appSettings.IsVerbose() ) _console.Write(" 👟 ");

				var comparedNamesList = new List<string>
				{
					nameof(FileIndexItem.Latitude).ToLowerInvariant(),
					nameof(FileIndexItem.Longitude).ToLowerInvariant(),
					nameof(FileIndexItem.LocationAltitude).ToLowerInvariant()
				};
                
				if(syncLocationNames) comparedNamesList.AddRange( new List<string>
				{
					nameof(FileIndexItem.LocationCity).ToLowerInvariant(),
					nameof(FileIndexItem.LocationState).ToLowerInvariant(),
					nameof(FileIndexItem.LocationCountry).ToLowerInvariant(),
					nameof(FileIndexItem.LocationCountryCode).ToLowerInvariant()
				});
				
				await _exifToolCmdHelper.UpdateAsync(metaFileItem, comparedNamesList);

				// Rocket man!
				_console.Write(_appSettings.IsVerbose()
					? $"  GeoLocationWrite: {metaFileItem.FilePath}  "
					: "🚀");
			}
		}
	}
}
