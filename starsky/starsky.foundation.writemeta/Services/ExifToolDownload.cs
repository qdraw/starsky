using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
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
	[SuppressMessage("Usage", "S1075:Refactor your code not to use hardcoded absolute paths or URIs", Justification = "Source of files")]
	public sealed class ExifToolDownload : IExifToolDownload
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

		/// <summary>
		/// Auto Download Exiftool
		/// </summary>
		/// <param name="isWindows">download Windows version if true</param>
		/// <param name="minimumSize">check for min file size in bytes (Default = 30 bytes)</param>
		/// <returns></returns>
		public async Task<bool> DownloadExifTool(bool isWindows, int minimumSize = 30)
		{
			if ( _appSettings.ExiftoolSkipDownloadOnStartup == true || _appSettings is { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
			{
				var name = _appSettings.ExiftoolSkipDownloadOnStartup == true
					? "ExiftoolSkipDownloadOnStartup"
					: "AddSwaggerExport and AddSwaggerExportExitAfter";
				_logger.LogInformation($"[DownloadExifTool] Skipped due true of {name} setting");
				return false;
			}

			CreateDirectoryDependenciesFolderIfNotExists();

			if ( isWindows &&
				 ( !_hostFileSystemStorage.ExistFile(ExeExifToolWindowsFullFilePath()) ||
				 _hostFileSystemStorage.Info(ExeExifToolWindowsFullFilePath()).Size <= minimumSize ) )
			{
				return await StartDownloadForWindows();
			}

			if ( !isWindows &&
				 ( !_hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) ||
				  _hostFileSystemStorage.Info(ExeExifToolUnixFullFilePath()).Size <= minimumSize ) )
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
			if ( isWindows ) return true;

			return await RunChmodOnExifToolUnixExe();
		}

		private void CreateDirectoryDependenciesFolderIfNotExists()
		{
			if ( _hostFileSystemStorage.ExistFolder(
				    _appSettings.DependenciesFolder) )
			{
				return;
			}
			_logger.LogInformation("[DownloadExifTool] Create Directory: " + _appSettings.DependenciesFolder);
			_hostFileSystemStorage.CreateDirectory(_appSettings.DependenciesFolder);
		}

		internal async Task<KeyValuePair<bool, string>?> DownloadCheckSums()
		{
			var baseLocationResult = await DownloadCheckSums(CheckSumLocation);
			if ( baseLocationResult == null )
			{
				return await DownloadCheckSums(CheckSumLocationMirror);
			}
			return baseLocationResult;
		}

		internal async Task<KeyValuePair<bool, string>?> DownloadCheckSums(string checkSumUrl)
		{
			var checksums = await _httpClientHelper.ReadString(checkSumUrl);
			if ( checksums.Key )
			{
				return checksums;
			}
			
			_logger.LogError($"Checksum loading failed {CheckSumLocation}, next retry from mirror ~ error > " + checksums.Value);
			return null;
		}

		internal async Task<bool> StartDownloadForUnix()
		{
			var checksums = await DownloadCheckSums();
			if ( checksums == null )
			{
				return false;
			}
			var matchExifToolForUnixName = GetUnixTarGzFromChecksum(checksums.Value.Value);
			return await DownloadForUnix(matchExifToolForUnixName,
				GetChecksumsFromTextFile(checksums.Value.Value));
		}

		internal static string GetUnixTarGzFromChecksum(string checksumsValue)
		{
			// (?<=SHA1\()Image-ExifTool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA256\()Image-ExifTool-[0-9\.]+\.tar.gz",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}

		private string ExeExifToolUnixFullFilePath()
		{
			var path = Path.Combine(_appSettings.DependenciesFolder,
					"exiftool-unix",
					"exiftool");
			return path;
		}

		internal async Task<bool> DownloadForUnix(string matchExifToolForUnixName, string[] getChecksumsFromTextFile)
		{
			var result = await DownloadForUnix(ExiftoolDownloadBasePath, matchExifToolForUnixName,
				getChecksumsFromTextFile);
			
			if ( result )
			{
				return true;
			}

			return await DownloadForUnix(ExiftoolDownloadBasePathMirror, matchExifToolForUnixName,
				getChecksumsFromTextFile);
		}

		
		private async Task<bool> DownloadForUnix(string exiftoolDownloadBasePath, string matchExifToolForUnixName,
			string[] getChecksumsFromTextFile)
		{

			if ( _hostFileSystemStorage.ExistFile(ExeExifToolUnixFullFilePath()) )
			{
				return true;
			}

			var tarGzArchiveFullFilePath = Path.Combine(_appSettings.DependenciesFolder, "exiftool.tar.gz");

			var url = $"{exiftoolDownloadBasePath}{matchExifToolForUnixName}";

			var unixDownloaded = await _httpClientHelper.Download(url, tarGzArchiveFullFilePath);
			if ( !unixDownloaded )
			{
				_logger.LogError($"file is not downloaded {matchExifToolForUnixName}");
				return false;
			}
			
			if ( !CheckSha256(tarGzArchiveFullFilePath, getChecksumsFromTextFile) )
			{
				_logger.LogError($"Checksum for {tarGzArchiveFullFilePath} is not valid");
				_hostFileSystemStorage.FileDelete(tarGzArchiveFullFilePath);
				return false;
			}

			await new TarBal(_hostFileSystemStorage).ExtractTarGz(
				_hostFileSystemStorage.ReadStream(tarGzArchiveFullFilePath), _appSettings.DependenciesFolder, CancellationToken.None);

			var imageExifToolVersionFolder = _hostFileSystemStorage.GetDirectories(_appSettings.DependenciesFolder)
				.FirstOrDefault(p => p.StartsWith(Path.Combine(_appSettings.DependenciesFolder, "Image-ExifTool-")));
			if ( imageExifToolVersionFolder != null )
			{
				var exifToolUnixFolderFullFilePath = Path.Combine(_appSettings.DependenciesFolder, "exiftool-unix");
				if ( _hostFileSystemStorage.ExistFolder(exifToolUnixFolderFullFilePath) )
				{
					_hostFileSystemStorage.FolderDelete(
						exifToolUnixFolderFullFilePath);
				}
				_hostFileSystemStorage.FolderMove(imageExifToolVersionFolder, exifToolUnixFolderFullFilePath);
			}
			else
			{
				_logger.LogError($"[DownloadForUnix] ExifTool folder does not exists");
				return false;
			}

			// remove tar.gz file afterwards
			_hostFileSystemStorage.FileDelete(tarGzArchiveFullFilePath);

			var exifToolExePath = Path.Combine(_appSettings.DependenciesFolder, "exiftool-unix", "exiftool");
			_logger.LogInformation($"[DownloadForUnix] ExifTool is just downloaded: {exifToolExePath} for {_appSettings.ApplicationType}");
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
			var result = await Command.Run("/bin/chmod", "0755", ExeExifToolUnixFullFilePath()).Task;
			if ( result.Success ) return true;

			_logger.LogError($"command failed with exit code {result.ExitCode}: {result.StandardError}");
			return false;
		}

		internal async Task<bool> StartDownloadForWindows()
		{
			var checksums = await DownloadCheckSums();
			if ( checksums == null ) return false;

			var matchExifToolForWindowsName = GetWindowsZipFromChecksum(checksums.Value.Value);
			return await DownloadForWindows(matchExifToolForWindowsName,
				GetChecksumsFromTextFile(checksums.Value.Value));
		}

		internal static string GetWindowsZipFromChecksum(string checksumsValue)
		{
			// (?<=SHA256\()exiftool-[\d\.]+_64\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA256\()exiftool-[0-9\.]+_64\.zip",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));
			return regexExifToolForWindowsName.Match(checksumsValue).Value;
		}

		/// <summary>
		/// Parse the content of checksum file
		/// </summary>
		/// <param name="checksumsValue">input file: see test for example</param>
		/// <param name="max">max number of SHA256 results</param>
		/// <returns></returns>
		internal string[] GetChecksumsFromTextFile(string checksumsValue, int max = 20)
		{
			// SHA256 = 64 characters, SHA1 = 40 characters
			var regexExifToolForWindowsName = new Regex("[a-z0-9]{64}", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100));
			var results = regexExifToolForWindowsName.Matches(checksumsValue).
				Select(m => m.Value).
				ToArray();
			if ( results.Length < max )
			{
				return results;
			}

			_logger.LogError($"More than {max} checksums found, this is not expected, code stops now");
			return [];
		}

		/// <summary>
		/// Check if SHA256 hash is valid
		/// Instead of SHA1CryptoServiceProvider, we use SHA256.Create
		/// </summary>
		/// <param name="fullFilePath">path of exiftool.exe</param>
		/// <param name="checkSumOptions">list of SHA256 hashes</param>
		/// <returns></returns>
		internal bool CheckSha256(string fullFilePath, IEnumerable<string> checkSumOptions)
		{
			using var buffer = _hostFileSystemStorage.ReadStream(fullFilePath);
			using var hashAlgorithm = SHA256.Create();

			var byteHash = hashAlgorithm.ComputeHash(buffer);
			var hash = BitConverter.ToString(byteHash).Replace("-", string.Empty).ToLowerInvariant();
			return checkSumOptions.AsEnumerable().Any(p => p.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
		}

		private string ExeExifToolWindowsFullFilePath()
		{
			return Path.Combine(Path.Combine(_appSettings.DependenciesFolder, "exiftool-windows"), "exiftool.exe");
		}

		internal async Task<bool> DownloadForWindows(string matchExifToolForWindowsName, string[] getChecksumsFromTextFile)
		{
			var result = await DownloadForWindows(ExiftoolDownloadBasePath, matchExifToolForWindowsName,
				getChecksumsFromTextFile);
			
			if ( result )
			{
				return true;
			}

			return await DownloadForWindows(ExiftoolDownloadBasePathMirror, matchExifToolForWindowsName,
				getChecksumsFromTextFile);
		}
		
		private async Task<bool> DownloadForWindows(string exiftoolDownloadBasePath, string matchExifToolForWindowsName,
			string[] getChecksumsFromTextFile)
		{
			if ( _hostFileSystemStorage.ExistFile(
				    ExeExifToolWindowsFullFilePath()) )
			{
				return true;
			}

			var zipArchiveFullFilePath = Path.Combine(_appSettings.DependenciesFolder, "exiftool.zip");
			var windowsExifToolFolder = Path.Combine(_appSettings.DependenciesFolder, "exiftool-windows");

			var url = $"{exiftoolDownloadBasePath}{matchExifToolForWindowsName}";
			var windowsDownloaded = await _httpClientHelper.Download(url, zipArchiveFullFilePath);
			if ( !windowsDownloaded )
			{
				_logger.LogError($"file is not downloaded {matchExifToolForWindowsName}");
				return false;
			}

			if ( !CheckSha256(zipArchiveFullFilePath, getChecksumsFromTextFile) )
			{
				_logger.LogError($"Checksum for {zipArchiveFullFilePath} is not valid");
				return false;
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
