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
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.writemeta.Services
{
	[Service(typeof(IExifToolDownload), InjectionLifetime = InjectionLifetime.Singleton)]
	public class ExifToolDownload : IExifToolDownload
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;
		private readonly IStorage _hostFileSystemStorage;
		private readonly IWebLogger _logger;

		private const string CheckSumLocation = "https://exiftool.org/checksums.txt";
		private const string CheckSumLocationMirror = "https://qdraw.nl/special/mirror/exiftool/checksums.txt";
		private const string ExiftoolDownloadBasePath = "https://exiftool.org/"; // with slash at the end
		private const string ExiftoolDownloadBasePathMirror = "https://qdraw.nl/special/mirror/exiftool/"; // with slash at the end

		public ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings, IWebLogger logger)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
			_logger = logger;
		}
		
		internal ExifToolDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings, IWebLogger logger, IStorage storage)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_hostFileSystemStorage = storage;
			_logger = logger;
		}

		public async Task<bool> DownloadExifTool(bool isWindows)
		{
			if ( _appSettings.AddSwaggerExport == true && _appSettings.AddSwaggerExportExitAfter == true )
			{
				_logger.LogInformation("[DownloadExifTool] Skipped due AddSwaggerExportExitAfter setting");
				return false;
			}
			
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

			if ( _appSettings.IsVerbose() )
			{
				var debugPath = isWindows ? ExeExifToolWindowsFullFilePath()
					: ExeExifToolUnixFullFilePath();
				_logger.LogInformation($"[DownloadExifTool] {debugPath}");
			}
			
			// When running deploy scripts rights might reset (only for unix)
			if ( isWindows) return true;

			return await RunChmodOnExifToolUnixExe();
		}

		internal async Task<KeyValuePair<bool, string>?> DownloadCheckSums()
		{
			var checksums = await _httpClientHelper.ReadString(CheckSumLocation);
			if ( checksums.Key )
			{
				return checksums;
			}
			_logger.LogError($"Checksum loading failed {CheckSumLocation}, next retry from mirror");
			checksums = await _httpClientHelper.ReadString(CheckSumLocationMirror);
			if ( checksums.Key ) return new KeyValuePair<bool, string>(false,checksums.Value);
			
			_logger.LogError($"Checksum loading failed {CheckSumLocationMirror}" +
			                 $", next stop; please connect to internet and restart app");
			return null;
		}

		internal async Task<bool> StartDownloadForUnix()
		{
			var checksums = await DownloadCheckSums();
			if ( checksums == null ) return false;
			var matchExifToolForUnixName = GetUnixTarGzFromChecksum(checksums?.Value);
			return await DownloadForUnix(matchExifToolForUnixName, 
				GetChecksumsFromTextFile(checksums.Value.Value), !checksums.Value.Key);
		}
		
		internal static string GetUnixTarGzFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()Image-ExifTool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()Image-ExifTool-[0-9\.]+\.tar.gz");
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}

		private string ExeExifToolUnixFullFilePath()
		{
			var path = Path.Combine(_appSettings.TempFolder, 
					"exiftool-unix",
					"exiftool");
			return path;
		}
		
		internal async Task<bool> DownloadForUnix(string matchExifToolForUnixName,
			string[] getChecksumsFromTextFile, bool downloadFromMirror = false)
		{

			if ( _hostFileSystemStorage.ExistFile(
				ExeExifToolUnixFullFilePath()) ) return true;
			
			var tarGzArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.tar.gz");

			var url = $"{ExiftoolDownloadBasePath}{matchExifToolForUnixName}";
			if ( downloadFromMirror ) url = $"{ExiftoolDownloadBasePathMirror}{matchExifToolForUnixName}";
			
			var unixDownloaded = await _httpClientHelper.Download(url, tarGzArchiveFullFilePath);
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
				var exifToolUnixFolderFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool-unix");
				_hostFileSystemStorage.FolderMove(imageExifToolVersionFolder,exifToolUnixFolderFullFilePath);
			}
			else
			{
				_logger.LogError($"[DownloadForUnix] ExifTool folder does not exists");
				return false;
			}
			
			var exifToolExePath = Path.Combine(_appSettings.TempFolder, "exiftool-unix","exiftool");
			_logger.LogInformation($"[DownloadForUnix] ExifTool downloaded: {exifToolExePath}");
			return await RunChmodOnExifToolUnixExe();
		}

		internal async Task<bool> RunChmodOnExifToolUnixExe()
		{
			// need to check again
			// when not exist
			if ( !_hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) ) return false;
			if ( _appSettings.IsWindows ) return true;
			
			if (! _hostFileSystemStorage.ExistFile("/bin/chmod") )
			{
				_logger.LogError("[RunChmodOnExifToolUnixExe] WARNING: /bin/chmod does not exist");
				return true;
			}
			
			// command.run does not care about the $PATH
			var result = await Command.Run("/bin/chmod","0755", ExeExifToolUnixFullFilePath()).Task; 
			if ( result.Success ) return true;
			
			_logger.LogError($"command failed with exit code {result.ExitCode}: {result.StandardError}");
			return false;
		}

		internal async Task<bool> StartDownloadForWindows()
		{
			var checksums = await DownloadCheckSums();
			if ( checksums == null ) return false;
			
			var matchExifToolForWindowsName = GetWindowsZipFromChecksum(checksums.Value.Value);
			return await DownloadForWindows(matchExifToolForWindowsName,GetChecksumsFromTextFile(checksums.Value.Value), !checksums.Value.Key);
		}

		internal static string GetWindowsZipFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()exiftool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()exiftool-[0-9\.]+\.zip");
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}
		
		private static string[] GetChecksumsFromTextFile(string checksumsValue)
		{
			var regexExifToolForWindowsName = new Regex("[a-z0-9]{40}");
			var results = regexExifToolForWindowsName.Matches(checksumsValue).
				Cast<Match>().
				Select(m => m.Value).
				ToArray();
			return results;
		}

		internal bool CheckSha1(string fullFilePath, IEnumerable<string> checkSumOptions)
		{
			using var buffer = _hostFileSystemStorage.ReadStream(fullFilePath);
			using var hashAlgorithm = SHA1.Create();
			
			var byteHash = hashAlgorithm.ComputeHash(buffer);
			var hash = BitConverter.ToString(byteHash).Replace("-",string.Empty).ToLowerInvariant();
			return checkSumOptions.AsEnumerable().Any(p => p.ToLowerInvariant() == hash);
		}

		private string ExeExifToolWindowsFullFilePath()
		{
			return Path.Combine(Path.Combine(_appSettings.TempFolder,"exiftool-windows"), "exiftool.exe");
		}

		internal async Task<bool> DownloadForWindows(string matchExifToolForWindowsName,
			string[] getChecksumsFromTextFile, bool downloadFromMirror = false)
		{
			if ( _hostFileSystemStorage.ExistFile(
				ExeExifToolWindowsFullFilePath()) ) return true;

			var zipArchiveFullFilePath = Path.Combine(_appSettings.TempFolder, "exiftool.zip");
			var windowsExifToolFolder = Path.Combine(_appSettings.TempFolder, "exiftool-windows");
			
			var url = $"{ExiftoolDownloadBasePath}{matchExifToolForWindowsName}";
			if ( downloadFromMirror ) url = $"{ExiftoolDownloadBasePathMirror}{matchExifToolForWindowsName}";
			
			var windowsDownloaded = await _httpClientHelper.Download(url, zipArchiveFullFilePath);
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

			_logger.LogInformation($"[DownloadForWindows] ExifTool downloaded: {ExeExifToolWindowsFullFilePath()}");
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
