using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.geolookup.Services;

/// <summary>
///     GeoCLI is a command line interface for the GeoLookup
///     To test: exiftool image.jpg -Country-PrimaryLocationName="" -Country="" -State=""
///     -Province-State=""
///     -City="" -xmp:City="" -XMP:CountryCode="" -Country-PrimaryLocationCode="" -overwrite_original
/// </summary>
public sealed class GeoCli
{
	private readonly AppSettings _appSettings;
	private readonly IConsole _console;
	private readonly IExifToolDownload _exifToolDownload;
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly IGeoFolderReverseLookup _geoFolderReverseLookup;
	private readonly IGeoLocationWrite _geoLocationWrite;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly ReadMeta _readMeta;
	private readonly IStorage _thumbnailStorage;

	[SuppressMessage("Usage",
		"S107: Constructor has 8 parameters, which is greater than the 7 authorized")]
	public GeoCli(IGeoFolderReverseLookup geoFolderReverseLookup,
		IGeoLocationWrite geoLocationWrite, ISelectorStorage selectorStorage,
		AppSettings appSettings, IConsole console,
		IGeoFileDownload geoFileDownload, IExifToolDownload exifToolDownload, IWebLogger logger)
	{
		_geoFolderReverseLookup = geoFolderReverseLookup;
		_geoLocationWrite = geoLocationWrite;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_readMeta = new ReadMeta(_iStorage, appSettings, null!, logger);
		_appSettings = appSettings;
		_console = console;
		_exifToolDownload = exifToolDownload;
		_geoFileDownload = geoFileDownload;
		_logger = logger;
	}

	/// <summary>
	///     Command line importer to Database and update disk
	/// </summary>
	/// <param name="args">arguments provided by command line app</param>
	/// <returns>Void Task</returns>
	public async Task CommandLineAsync(string[] args)
	{
		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
		// Set type of GeoReverseLookup
		_appSettings.ApplicationType = AppSettings.StarskyAppType.Geo;

		// Download ExifTool 
		await _exifToolDownload.DownloadExifTool(_appSettings.IsWindows);

		// Geo cities1000 download
		if ( _appSettings.GeoFilesSkipDownloadOnStartup != true )
		{
			await _geoFileDownload.DownloadAsync();
		}

		if ( ArgsHelper.NeedHelp(args) ||
		     ( new ArgsHelper(_appSettings).GetPathFormArgs(args, false).Length <= 1
		       && ArgsHelper.GetSubPathFormArgs(args).Length <= 1
		       && new ArgsHelper(_appSettings).GetRelativeValue(args) == null ) )
		{
			new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
			return;
		}

		// Using both options
		string inputPath;
		// -s = if subPath || -p is path
		if ( ArgsHelper.IsSubPathOrPath(args) )
		{
			var path = _appSettings.DatabasePathToFilePath(
				ArgsHelper.GetSubPathFormArgs(args)
			);
			inputPath = !string.IsNullOrEmpty(path) ? path : string.Empty;
		}
		else
		{
			inputPath = new ArgsHelper(_appSettings).GetPathFormArgs(args, false);
		}

		// overwrite subPath with relative days
		// use -g or --SubPathRelative to use it.
		// envs are not supported
		var getSubPathRelative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		if ( getSubPathRelative != null )
		{
			var dateTime = DateTime.Now.AddDays(( double ) getSubPathRelative);
			var path = _appSettings.DatabasePathToFilePath(
				new StructureService(_iStorage, _appSettings.Structure.DefaultPattern)
					.ParseSubfolders(dateTime));
			inputPath = !string.IsNullOrEmpty(path) ? path : string.Empty;
		}

		// used in this session to find the files back
		_appSettings.StorageFolder = inputPath;

		if ( inputPath == null || _iStorage.IsFolderOrFile("/") ==
		    FolderOrFileModel.FolderOrFileTypeList.Deleted )
		{
			_console.WriteLine("Folder location is not found \n" +
			                   $"Please try the `-h` command to get help \nDid search for: {inputPath}");
			return;
		}

		// use relative to StorageFolder
		var listOfFiles = _iStorage.GetAllFilesInDirectory("/")
			.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

		var fileIndexList = await _readMeta.ReadExifAndXmpFromFileAddFilePathHashAsync(listOfFiles);

		var toMetaFilesUpdate = new List<FileIndexItem>();
		if ( ArgsHelper.GetIndexMode(args) )
		{
			_console.WriteLine($"CameraTimeZone: {_appSettings.CameraTimeZone}");
			_console.WriteLine($"Folder: {inputPath}");

			var geoIndexGpx = new GeoIndexGpx(_appSettings, _iStorage, _logger);
			toMetaFilesUpdate = await geoIndexGpx.LoopFolderAsync(fileIndexList);

			_console.Write("¬");
			await _geoLocationWrite.LoopFolderAsync(toMetaFilesUpdate, false);
			_console.Write("(gps added)");
		}

		fileIndexList = await _geoFolderReverseLookup.LoopFolderLookup(fileIndexList,
			ArgsHelper.GetAll(args));

		if ( fileIndexList.Count >= 1 )
		{
			_console.Write("~ Add city, state and country info ~");

			await _geoLocationWrite.LoopFolderAsync(fileIndexList, true);
		}

		_console.Write("^\n");
		_console.Write("~ Rename thumbnails ~");

		// Loop though all options
		fileIndexList.AddRange(toMetaFilesUpdate);

		await RenameFileHash(fileIndexList);
		// dont updated in the database
		// that's not the scope of this command
	}

	private async Task RenameFileHash(IEnumerable<FileIndexItem> fileIndexList)
	{
		// update thumbs to avoid unnecessary re-generation
		foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
			         .ToList() )
		{
			var newThumb =
				( await new FileHash(_iStorage, _logger).GetHashCodeAsync(item.FilePath!) ).Key;
			if ( item.FileHash == newThumb )
			{
				continue;
			}

			new ThumbnailFileMoveAllSizes(_thumbnailStorage, _appSettings, _logger).FileMove(
				item.FileHash!, newThumb, item.FilePath);
			if ( _appSettings.IsVerbose() )
			{
				_console.WriteLine("thumb+ `" + item.FileHash + "`" + newThumb);
			}
		}
	}
}
