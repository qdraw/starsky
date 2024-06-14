using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.writemeta.Services.ExifToolDownloader;

[Service(typeof(IExifToolDownload), InjectionLifetime = InjectionLifetime.Singleton)]
[SuppressMessage("Usage",
	"S1075:Refactor your code not to use hardcoded absolute paths or URIs",
	Justification = "Source of files")]
[SuppressMessage("Usage",
	"S4790:Make sure this weak hash algorithm is not used in a sensitive context here.",
	Justification = "Safe")]
public sealed class ExifToolDownload : IExifToolDownload
{
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly ExifToolLocations _exifToolLocations;
	private readonly ExifToolDownloadUnix _exifToolDownloadUnix;

	public ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
		_exifToolLocations = new ExifToolLocations(_appSettings);
		_exifToolDownloadUnix = new ExifToolDownloadUnix(_hostFileSystemStorage, appSettings, httpClientHelper, logger);
		_exifToolDownloadWindows = new ExifToolDownloadWindows(_hostFileSystemStorage, appSettings, httpClientHelper, logger);

	}

	internal ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger, IStorage storage)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = storage;
		_logger = logger;
		_exifToolLocations = new ExifToolLocations(_appSettings);
		_exifToolDownloadUnix = new ExifToolDownloadUnix(_hostFileSystemStorage, appSettings, httpClientHelper, logger);
	}

	/// <summary>
	/// Auto Download Exiftool
	/// </summary>
	/// <param name="isWindows">download windows version if true</param>
	/// <param name="minimumSize">check for min file size in bytes (Default = 30 bytes)</param>
	/// <returns></returns>
	public async Task<bool> DownloadExifTool(bool isWindows, int minimumSize = 30)
	{
		if ( _appSettings.ExiftoolSkipDownloadOnStartup == true || _appSettings is
			    { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
		{
			var name = _appSettings.ExiftoolSkipDownloadOnStartup == true
				? "ExiftoolSkipDownloadOnStartup"
				: "AddSwaggerExport and AddSwaggerExportExitAfter";
			_logger.LogInformation($"[DownloadExifTool] Skipped due true of {name} setting");
			return false;
		}

		new CreateFolderIfNotExists(_logger, _appSettings)
			.CreateDirectoryDependenciesTempFolderIfNotExists();

		if ( isWindows &&
		     ( !_hostFileSystemStorage.ExistFile(
			       _exifToolLocations.ExeExifToolWindowsFullFilePath()) ||
		       _hostFileSystemStorage.Info(_exifToolLocations.ExeExifToolWindowsFullFilePath())
			       .Size <=
		       minimumSize ) )
		{
			return await StartDownloadForWindows();
		}

		if ( !isWindows &&
		     ( !_hostFileSystemStorage.ExistFile(_exifToolLocations
			       .ExeExifToolUnixFullFilePath()) ||
		       _hostFileSystemStorage.Info(_exifToolLocations.ExeExifToolUnixFullFilePath()).Size <=
		       minimumSize ) )
		{
			return await _exifToolDownloadUnix.StartDownloadForUnix();
		}

		if ( _appSettings.IsVerbose() )
		{
			_logger.LogInformation(
				$"[DownloadExifTool] {_exifToolLocations.ExeExifToolFullFilePath(isWindows)}");
		}

		// When running deploy scripts rights might reset (only for unix)
		if ( isWindows ) return true;

		return await _exifToolDownloadUnix.RunChmodOnExifToolUnixExe();
	}




}
