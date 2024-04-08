using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.writemeta.Services.ExifToolDownloader;

public class ExifToolDownloadUnix
{
	private readonly IStorage _hostFileSystemStorage;
	private readonly AppSettings _appSettings;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly ExifToolLocations _exifToolLocations;
	private readonly CheckSums _checkSha1;

	public ExifToolDownloadUnix(IStorage hostFileSystemStorage, AppSettings appSettings,
		IHttpClientHelper httpClientHelper, IWebLogger logger)
	{
		_hostFileSystemStorage = hostFileSystemStorage;
		_appSettings = appSettings;
		_httpClientHelper = httpClientHelper;
		_logger = logger;
		_exifToolLocations = new ExifToolLocations(_appSettings);
		_checkSha1 = new CheckSums(_hostFileSystemStorage, httpClientHelper, logger);
	}
	
	internal async Task<bool> StartDownloadForUnix()
	{
		var checksums = await DownloadCheckSums();
		if ( checksums == null ) return false;
		var matchExifToolForUnixName = GetUnixTarGzFromChecksum(checksums.Value.Value);
		return await DownloadForUnix(matchExifToolForUnixName,
			GetChecksumsFromTextFile(checksums.Value.Value), !checksums.Value.Key);
	}
	
	
	internal async Task<bool> DownloadForUnix(string matchExifToolForUnixName,
		IEnumerable<string> getChecksumsFromTextFile, bool downloadFromMirror = false)
	{
		if ( _hostFileSystemStorage.ExistFile(_exifToolLocations.ExeExifToolUnixFullFilePath()) )
		{
			return true;
		}

		var tarGzArchiveFullFilePath =
			Path.Combine(_appSettings.TempFolder, "exiftool.tar.gz");

		var url = $"{ExifToolLocations.ExiftoolDownloadBasePath}{matchExifToolForUnixName}";
		if ( downloadFromMirror )
			url = $"{ExifToolLocations.ExiftoolDownloadBasePathMirror}{matchExifToolForUnixName}";

		var unixDownloaded = await _httpClientHelper.Download(url, tarGzArchiveFullFilePath);
		if ( !unixDownloaded )
		{
			throw new HttpRequestException(
				$"file is not downloaded {matchExifToolForUnixName}");
		}

		if ( !_checkSha1.CheckSha1(tarGzArchiveFullFilePath, getChecksumsFromTextFile) )
		{
			throw new HttpRequestException(
				$"checksum for {tarGzArchiveFullFilePath} is not valid");
		}

		await new TarBal(_hostFileSystemStorage).ExtractTarGz(
			_hostFileSystemStorage.ReadStream(tarGzArchiveFullFilePath),
			_appSettings.TempFolder, CancellationToken.None);

		var imageExifToolVersionFolder = _hostFileSystemStorage
			.GetDirectories(_appSettings.TempFolder)
			.FirstOrDefault(p =>
				p.StartsWith(Path.Combine(_appSettings.TempFolder, "Image-ExifTool-")));
		if ( imageExifToolVersionFolder != null )
		{
			var exifToolUnixFolderFullFilePathTempFolder =
				Path.Combine(_appSettings.TempFolder, "exiftool-unix");

			if ( _hostFileSystemStorage.ExistFolder(exifToolUnixFolderFullFilePathTempFolder) )
			{
				_hostFileSystemStorage.FolderDelete(
					exifToolUnixFolderFullFilePathTempFolder);
			}

			_hostFileSystemStorage.FolderMove(imageExifToolVersionFolder,
				exifToolUnixFolderFullFilePathTempFolder);

			var exifToolUnixFolderFullFilePath =
				Path.Combine(_appSettings.DependenciesFolder, "exiftool-unix");

			_hostFileSystemStorage.FileCopy(imageExifToolVersionFolder,
				exifToolUnixFolderFullFilePathTempFolder);
		}
		else
		{
			_logger.LogError($"[DownloadForUnix] ExifTool folder does not exists");
			return false;
		}

		// remove tar.gz file afterwards
		_hostFileSystemStorage.FileDelete(tarGzArchiveFullFilePath);

		var exifToolExePath =
			Path.Combine(_appSettings.DependenciesFolder, "exiftool-unix", "exiftool");
		_logger.LogInformation(
			$"[DownloadForUnix] ExifTool is just downloaded: {exifToolExePath} for {_appSettings.ApplicationType}");
		return await RunChmodOnExifToolUnixExe();
	}

	internal async Task<bool> RunChmodOnExifToolUnixExe()
	{
		// need to check again
		// when not exist
		if ( !_hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) ) return false;
		if ( _appSettings.IsWindows ) return true;

		if ( !_hostFileSystemStorage.ExistFile("/bin/chmod") )
		{
			_logger.LogError("[RunChmodOnExifToolUnixExe] WARNING: /bin/chmod does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run("/bin/chmod", "0755", ExeExifToolUnixFullFilePath())
			.Task;
		if ( result.Success ) return true;

		_logger.LogError(
			$"command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}
