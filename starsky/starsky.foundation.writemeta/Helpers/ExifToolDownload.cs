using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Medallion.Shell;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.writemeta.Helpers
{
	public class ExifToolDownload
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;
		private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;

		public ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_hostFileSystemStorage = new StorageHostFullPathFilesystem();
		}

		public async Task<bool> DownloadExifTool()
		{
			var checksums = await _httpClientHelper.ReadString("https://exiftool.org/checksums.txt");
			if ( !checksums.Key )
			{
				return false;
			}
			if ( _appSettings.IsWindows )
			{
				return await StartDownloadForWindows(checksums.Value);
			}
			return await StartDownloadForUnix(checksums.Value);
		}

		internal async Task<bool> StartDownloadForUnix(string checksumsValue)
		{
			var matchExifToolForUnixName = GetUnixTarGzFromChecksum(checksumsValue);
			return await DownloadForUnix(matchExifToolForUnixName, GetChecksumsFromTextFile(checksumsValue));
		}
		
		internal string GetUnixTarGzFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()Image-ExifTool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()Image-ExifTool-[0-9\.]+\.tar.gz");
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}
		
		private async Task<bool> DownloadForUnix(string matchExifToolForUnixName,
			string[] getChecksumsFromTextFile)
		{

			var tarGzArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.tar.gz");

			var unixDownloaded = await _httpClientHelper.Download(
				$"https://exiftool.org/{matchExifToolForUnixName}", tarGzArchiveFullFilePath);
			if ( !unixDownloaded ) return false;
			if ( !CheckSha1(tarGzArchiveFullFilePath, getChecksumsFromTextFile) ) 
			{
				throw new HttpRequestException($"checksum for {tarGzArchiveFullFilePath} is not valid");
			}
			
			var exifToolUnixFolderFullFilePath = Path.Combine(_appSettings.TempFolder,"exiftool-unix");
			var exeExifToolUnixFullFilePath =
				Path.Combine(exifToolUnixFolderFullFilePath, "exiftool");

			var existExeExifToolUnixFullFilePath =
				_hostFileSystemStorage.ExistFile(exeExifToolUnixFullFilePath);
			if ( existExeExifToolUnixFullFilePath ) return true;
			
			new TarBal(_hostFileSystemStorage).ExtractTarGz(_hostFileSystemStorage.ReadStream(tarGzArchiveFullFilePath), _appSettings.TempFolder);
			
			var imageExifToolVersionFolder = _hostFileSystemStorage.GetDirectories(_appSettings.TempFolder)
				.FirstOrDefault(p => p.StartsWith(Path.Combine(_appSettings.TempFolder, "Image-ExifTool-")));
			if ( imageExifToolVersionFolder != null )
			{
				_hostFileSystemStorage.FolderMove(imageExifToolVersionFolder,exifToolUnixFolderFullFilePath);
			}

			// need to check again
			if ( !_hostFileSystemStorage.ExistFile(exeExifToolUnixFullFilePath) ) return false;

			if ( _appSettings.IsWindows ) return true;
			
			var result = await Command.Run("chmod","0755", $"{exeExifToolUnixFullFilePath}").Task;
			if ( result.Success ) return true;
			await Console.Error.WriteLineAsync($"command failed with exit code {result.ExitCode}: {result.StandardError}");
			return false;
		}

		internal async Task<bool> StartDownloadForWindows(string checksumsValue)
		{
			var matchExifToolForWindowsName = GetWindowsZipFromChecksum(checksumsValue);
			return await DownloadForWindows(matchExifToolForWindowsName,GetChecksumsFromTextFile(checksumsValue));
		}

		internal string GetWindowsZipFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()exiftool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()exiftool-[0-9\.]+\.zip");
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}
		
		internal string[] GetChecksumsFromTextFile(string checksumsValue)
		{
			var regexExifToolForWindowsName = new Regex("[a-z0-9]{40}");
			var results = regexExifToolForWindowsName.Matches(checksumsValue).
				Cast<Match>().
				Select(m => m.Value).
				ToArray();
			return results;
		}

		private bool CheckSha1(string fullFilePath, string[] checkSumOptions)
		{
			using ( var buffer = _hostFileSystemStorage.ReadStream(fullFilePath) )
			using(var cryptoProvider = new SHA1CryptoServiceProvider())
			{
				var hash = BitConverter
					.ToString(cryptoProvider.ComputeHash(buffer)).Replace("-","").ToLowerInvariant();
				return checkSumOptions.AsEnumerable().Any(p => p.ToLowerInvariant() == hash);
			}
		}

		private async Task<bool> DownloadForWindows(string matchExifToolForWindowsName,
			string[] getChecksumsFromTextFile)
		{
			var zipArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.zip");
			var windowsExifToolFolder = Path.Combine(_appSettings.TempFolder, "exiftool-windows");

			if ( _hostFileSystemStorage.ExistFile(Path.Combine(windowsExifToolFolder, "exiftool.exe")) ) return true;
			
			var windowsDownloaded = await _httpClientHelper.Download(
				$"https://exiftool.org/{matchExifToolForWindowsName}", zipArchiveFullFilePath);
			if ( !windowsDownloaded ) return false;
			if ( !CheckSha1(zipArchiveFullFilePath, getChecksumsFromTextFile) ) 
			{
				throw new HttpRequestException($"checksum for {zipArchiveFullFilePath} is not valid");
			}
			
			_hostFileSystemStorage.CreateDirectory(windowsExifToolFolder);

			new Zipper().ExtractZip(zipArchiveFullFilePath, windowsExifToolFolder);
			MoveFileIfExist(Path.Combine(windowsExifToolFolder, "exiftool(-k).exe"),
				Path.Combine(windowsExifToolFolder, "exiftool.exe"));
			MoveFileIfExist(Path.Combine(windowsExifToolFolder, "exiftool(-k).obj"),
				Path.Combine(windowsExifToolFolder, "exiftool.obj"));

			return _hostFileSystemStorage.ExistFile(Path.Combine(windowsExifToolFolder,
				"exiftool.exe"));
		}

		private void MoveFileIfExist(string srcFullPath, string toFullPath)
		{
			if ( !_hostFileSystemStorage.ExistFile(srcFullPath) ) return;
			_hostFileSystemStorage.FileMove(srcFullPath, toFullPath);
		}
	}
}
