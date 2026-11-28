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
using starsky.foundation.storage.Structure;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.platform.Tenancy;

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
	private readonly IWebLogger _logger;
	private readonly ReadMeta _readMeta;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IStorage _storage;
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
		_selectorStorage = selectorStorage;
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_readMeta = new ReadMeta(_storage, appSettings, null!, logger);
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
		var isTenant = TenantAmbient.Current != TenantIdHelper.DefaultTenantId;
		if ( isTenant && ArgsHelper.IsFullPathInArgs(args) )
		{
			// a full path is converted with the folders of the default tenant
			_console.WriteLine("--path (a full path) can not be used with --tenant, use --subpath");
			return;
		}

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

		if ( ArgsHelper.NeedHelp(args) || NoFolderInArgs(args, isTenant) )
		{
			new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
			return;
		}

		// the folder to work on, as a subPath of the storage of the tenant
		var (inputPath, folderSubPath) = isTenant
			? TenantFolder(args)
			: DefaultTenantFolder(args);

		if ( _storage.IsFolderOrFile(folderSubPath) ==
		     FolderOrFileModel.FolderOrFileTypeList.Deleted )
		{
			_console.WriteLine("Folder location is not found \n" +
			                   $"Please try the `-h` command to get help \nDid search for: {inputPath}");
			return;
		}

		// use relative to StorageFolder (the default tenant) or the storage of the tenant
		var listOfFiles = _storage.GetAllFilesInDirectory(folderSubPath)
			.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

		var fileIndexList = await _readMeta.ReadExifAndXmpFromFileAddFilePathHashAsync(listOfFiles);

		var toMetaFilesUpdate = new List<FileIndexItem>();
		if ( ArgsHelper.GetIndexMode(args) )
		{
			_console.WriteLine($"CameraTimeZone: {_appSettings.CameraTimeZone}");
			_console.WriteLine($"Folder: {inputPath}");

			var geoIndexGpx = new GeoIndexGpx(_appSettings, _storage, _logger);
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
		// don't update in the database
		// that's not the scope of this command
	}

	/// <summary>
	///     No folder, and no relative date, in the arguments: show the help. A full path is not
	///     read with a tenant (it throws), only the subPath is
	/// </summary>
	private bool NoFolderInArgs(string[] args, bool isTenant)
	{
		var argsHelper = new ArgsHelper(_appSettings);
		return ( isTenant || argsHelper.GetPathFormArgs(args, false).Length <= 1 )
		       && ArgsHelper.GetSubPathFormArgs(args).Length <= 1
		       && argsHelper.GetRelativeValue(args) == null;
	}

	/// <summary>
	///     The default tenant: this tool points the storage folder of the session at the given
	///     folder, so that folder is the root of the storage
	/// </summary>
	private (string InputPath, string FolderSubPath) DefaultTenantFolder(string[] args)
	{
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
			inputPath = _appSettings.DatabasePathToFilePath(
				new StructureService(_selectorStorage, _appSettings, _logger)
					.ParseSubfolders(getSubPathRelative));
		}

		// used in this session to find the files back
		_appSettings.StorageFolder = inputPath;
		return ( inputPath, "/" );
	}

	/// <summary>
	///     A tenant has its own storage folder (not the folder of the settings of the session):
	///     work on a subPath in it, and leave the storage folder alone
	/// </summary>
	private (string InputPath, string FolderSubPath) TenantFolder(string[] args)
	{
		var subPath = ArgsHelper.GetSubPathFormArgs(args);

		// use -g or --SubPathRelative to use it.
		var getSubPathRelative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		if ( getSubPathRelative != null )
		{
			subPath = new StructureService(_selectorStorage, _appSettings, _logger)
				.ParseSubfolders(getSubPathRelative);
		}

		subPath = PathHelper.PrefixDbSlash(subPath);
		return ( subPath, subPath );
	}

	private async Task RenameFileHash(IEnumerable<FileIndexItem> fileIndexList)
	{
		// update thumbs to avoid unnecessary re-generation
		foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
			         .ToList() )
		{
			var fileHashService = new FileHash(_storage, _logger);
			var newThumb =
				( await fileHashService.GetHashCodeAsync(item.FilePath!,
					item.ImageFormat) ).Key;
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
