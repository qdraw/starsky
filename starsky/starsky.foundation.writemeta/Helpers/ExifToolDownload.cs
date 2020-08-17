using System;
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

		private const string CheckSumLocation = "https://exiftool.org/checksums.txt";
		
		public ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_hostFileSystemStorage = new StorageHostFullPathFilesystem();
		}

		public async Task<bool> DownloadExifTool(bool isWindows)
		{
			if ( isWindows &&
			     !_hostFileSystemStorage.ExistFile(ExeExifToolWindowsFullFilePath ()) )
			{
				return await StartDownloadForWindows();
			}

			if ( !isWindows &&
			     !_hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()))
			{
				return await StartDownloadForUnix();
			}
			
			// When running deploy scripts rights will reset
			if ( !isWindows )
			{
				return await RunChmodOnExifToolUnixExe();
			}

			return true;
		}

		internal async Task<bool> StartDownloadForUnix()
		{
			var checksums = await _httpClientHelper.ReadString(CheckSumLocation);
			if ( !checksums.Key ) return false;
			var matchExifToolForUnixName = GetUnixTarGzFromChecksum(checksums.Value);
			return await DownloadForUnix(matchExifToolForUnixName, GetChecksumsFromTextFile(checksums.Value));
		}
		
		internal string GetUnixTarGzFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()Image-ExifTool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()Image-ExifTool-[0-9\.]+\.tar.gz");
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}

		private string ExeExifToolUnixFullFilePath()
		{
			var path = Path.Combine(Path.Combine(_appSettings.TempFolder,"exiftool-unix"), "exiftool");
			return path;
		}
		
		private async Task<bool> DownloadForUnix(string matchExifToolForUnixName,
			string[] getChecksumsFromTextFile)
		{

			if ( _hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) ) return true;
			
			var tarGzArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.tar.gz");
			var unixDownloaded = await _httpClientHelper.Download(
				$"https://exiftool.org/{matchExifToolForUnixName}", tarGzArchiveFullFilePath);
			if ( !unixDownloaded )
			{
				throw new HttpRequestException($"file is not downloaded {matchExifToolForUnixName}");
			}
			if ( !CheckSha1(tarGzArchiveFullFilePath, getChecksumsFromTextFile) ) 
			{
				throw new HttpRequestException($"checksum for {tarGzArchiveFullFilePath} is not valid");
			}
			
			new TarBal(_hostFileSystemStorage).ExtractTarGz(_hostFileSystemStorage.ReadStream(tarGzArchiveFullFilePath), _appSettings.TempFolder);
			
			var imageExifToolVersionFolder = _hostFileSystemStorage.GetDirectories(_appSettings.TempFolder)
				.FirstOrDefault(p => p.StartsWith(Path.Combine(_appSettings.TempFolder, "Image-ExifTool-")));
			if ( imageExifToolVersionFolder != null )
			{
				var exifToolUnixFolderFullFilePath =
					Path.Combine(_appSettings.TempFolder, "exiftool-unix");
				_hostFileSystemStorage.FolderMove(imageExifToolVersionFolder,exifToolUnixFolderFullFilePath);
			}

			return await RunChmodOnExifToolUnixExe();
		}

		private async Task<bool> RunChmodOnExifToolUnixExe()
		{
			// need to check again
			if ( !_hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) ) return false;
			if ( _appSettings.IsWindows ) return true;
			var result = await Command.Run("chmod","0755", $"{ExeExifToolUnixFullFilePath()}").Task;
			if ( result.Success ) return true;
			await Console.Error.WriteLineAsync($"command failed with exit code {result.ExitCode}: {result.StandardError}");
			return false;
		}

		internal async Task<bool> StartDownloadForWindows()
		{
			var checksums = await _httpClientHelper.ReadString(CheckSumLocation);
			if ( !checksums.Key ) return false;
			var matchExifToolForWindowsName = GetWindowsZipFromChecksum(checksums.Value);
			return await DownloadForWindows(matchExifToolForWindowsName,GetChecksumsFromTextFile(checksums.Value));
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

		private string ExeExifToolWindowsFullFilePath()
		{
			return Path.Combine(Path.Combine(_appSettings.TempFolder,"exiftool-windows"), "exiftool.exe");
		}
		
		private async Task<bool> DownloadForWindows(string matchExifToolForWindowsName,
			string[] getChecksumsFromTextFile)
		{
			if ( _hostFileSystemStorage.ExistFile(ExeExifToolWindowsFullFilePath()) ) return true;

			var zipArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.zip");
			var windowsExifToolFolder = Path.Combine(_appSettings.TempFolder, "exiftool-windows");
			
			var windowsDownloaded = await _httpClientHelper.Download(
				$"https://exiftool.org/{matchExifToolForWindowsName}", zipArchiveFullFilePath);
			if ( !windowsDownloaded )
			{
				throw new HttpRequestException($"file is not downloaded {matchExifToolForWindowsName}");
			}
			
			if ( !CheckSha1(zipArchiveFullFilePath, getChecksumsFromTextFile) ) 
			{
				throw new HttpRequestException($"checksum for {zipArchiveFullFilePath} is not valid");
			}
			
			_hostFileSystemStorage.CreateDirectory(windowsExifToolFolder);

			new Zipper().ExtractZip(zipArchiveFullFilePath, windowsExifToolFolder);
			MoveFileIfExist(Path.Combine(windowsExifToolFolder, "exiftool(-k).exe"),
				Path.Combine(windowsExifToolFolder, "exiftool.exe"));

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
