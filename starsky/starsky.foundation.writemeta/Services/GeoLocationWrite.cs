using System.Collections.Generic;
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
	public class GeoLocationWrite : IGeoLocationWrite
	{
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IConsole _console;

		public GeoLocationWrite(AppSettings appSettings, IExifTool exifTool, ISelectorStorage selectorStorage, IConsole console)
		{
			_exifTool = exifTool;
			_appSettings = appSettings;
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_console = console;
		}
        
		/// <summary>
		/// Write to ExifTool by list
		/// </summary>
		/// <param name="metaFilesInDirectory">list of files with data</param>
		/// <param name="syncLocationNames">Write city, state and country to exifTool (false > no)</param>
		public void LoopFolder(List<FileIndexItem> metaFilesInDirectory, bool syncLocationNames)
		{
			foreach (var metaFileItem in metaFilesInDirectory)
			{
				if (!ExtensionRolesHelper.IsExtensionExifToolSupported(metaFileItem.FileName)) continue;

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
				});
                
				new ExifToolCmdHelper(_exifTool, 
					_iStorage, 
					_thumbnailStorage, 
					new ReadMeta(_iStorage)).Update(metaFileItem, comparedNamesList);

				_console.Write(_appSettings.IsVerbose()
					? $"  GeoLocationWrite: {metaFileItem.FilePath}  "
					: "🚀");
			}

		}
	}
}
