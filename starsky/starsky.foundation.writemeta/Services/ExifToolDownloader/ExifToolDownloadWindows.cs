using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.writemeta.Services.ExifToolDownloader;

public class ExifToolDownloadWindows(IStorage hostFileSystemStorage, AppSettings appSettings,
	IHttpClientHelper httpClientHelper, IWebLogger logger)
{
	internal async Task<bool> StartDownloadForWindows()
	{
		var checksums = await DownloadCheckSums();
		if ( checksums == null ) return false;

		var matchExifToolForWindowsName = GetWindowsZipFromChecksum(checksums.Value.Value);
		return await DownloadForWindows(matchExifToolForWindowsName,
			GetChecksumsFromTextFile(checksums.Value.Value), !checksums.Value.Key);
	}
	
	
	private void MoveFileIfExist(string srcFullPath, string toFullPath)
	{
		if ( !_hostFileSystemStorage.ExistFile(srcFullPath) ) return;
		_hostFileSystemStorage.FileMove(srcFullPath, toFullPath);
	}

	internal async Task<bool> DownloadForWindows(string matchExifToolForWindowsName,
		IEnumerable<string> getChecksumsFromTextFile, bool downloadFromMirror = false)
	{
		if ( _hostFileSystemStorage.ExistFile(
			    ExeExifToolWindowsFullFilePath()) ) return true;

		var zipArchiveFullFilePath =
			Path.Combine(_appSettings.DependenciesFolder, "exiftool.zip");
		var windowsExifToolFolder =
			Path.Combine(_appSettings.DependenciesFolder, "exiftool-windows");

		var url = $"{ExiftoolDownloadBasePath}{matchExifToolForWindowsName}";
		if ( downloadFromMirror )
			url = $"{ExiftoolDownloadBasePathMirror}{matchExifToolForWindowsName}";

		var windowsDownloaded = await _httpClientHelper.Download(url, zipArchiveFullFilePath);
		if ( !windowsDownloaded )
		{
			throw new HttpRequestException(
				$"file is not downloaded {matchExifToolForWindowsName}");
		}

		if ( !CheckSha1(zipArchiveFullFilePath, getChecksumsFromTextFile) )
		{
			throw new HttpRequestException(
				$"checksum for {zipArchiveFullFilePath} is not valid");
		}

		_hostFileSystemStorage.CreateDirectory(windowsExifToolFolder);

		new Zipper().ExtractZip(zipArchiveFullFilePath, windowsExifToolFolder);
		MoveFileIfExist(Path.Combine(windowsExifToolFolder, "exiftool(-k).exe"),
			Path.Combine(windowsExifToolFolder, "exiftool.exe"));

		_logger.LogInformation(
			$"[DownloadForWindows] ExifTool downloaded: {ExeExifToolWindowsFullFilePath()}");
		return _hostFileSystemStorage.ExistFile(Path.Combine(windowsExifToolFolder,
			"exiftool.exe"));
	}
}
