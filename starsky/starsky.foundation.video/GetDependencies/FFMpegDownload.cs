using Medallion.Shell;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfMpegDownload : IFfMpegDownload
{
	private const string FfmpegDependenciesFolder = "ffmpeg";
	private readonly AppSettings _appSettings;
	private readonly FfMpegDownloadIndex _downloadIndex;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly MacCodeSign _macCodeSign;

	public FfMpegDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
		_downloadIndex = new FfMpegDownloadIndex(_httpClientHelper, _logger);
		_macCodeSign = new MacCodeSign(_hostFileSystemStorage, _logger);
	}

	public async Task<bool> DownloadFfMpeg()
	{
		if ( _appSettings.FfmpegSkipDownloadOnStartup == true || _appSettings is
			    { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
		{
			var name = _appSettings.FfmpegSkipDownloadOnStartup == true
				? "FFMpegSkipDownloadOnStartup"
				: "AddSwaggerExport and AddSwaggerExportExitAfter";
			_logger.LogInformation($"[DownloadFFMpeg] Skipped due true of {name} setting");
			return false;
		}

		CreateDirectoryDependenciesFolderIfNotExists();

		var currentArchitecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		// var currentArchitecture = "osx-x64";

		if ( _hostFileSystemStorage.ExistFile(GetExePath(currentArchitecture)) )
		{
			return true;
		}

		var container = await _downloadIndex.DownloadIndex();

		var binaryIndexBaseUrls = GetCurrentArchitectureIndexUrls(container,
			currentArchitecture);

		await Download(binaryIndexBaseUrls, currentArchitecture);

		await PrepareBeforeRunning(currentArchitecture);

		return true;
	}

	private static KeyValuePair<BinaryIndex?, List<Uri>> GetCurrentArchitectureIndexUrls(
		FfmpegBinariesContainer container,
		string currentArchitecture)
	{
		var sortedData = container.Data?.Binaries.Find(p =>
			p.Architecture == currentArchitecture);

		return new KeyValuePair<BinaryIndex?, List<Uri>>(sortedData, container.BaseUrls);
	}

	private async Task<bool?> Download(
		KeyValuePair<BinaryIndex?, List<Uri>> binaryIndexKeyValuePair, string currentArchitecture,
		int retryInSeconds = 15)
	{
		var (binaryIndex, baseUrls) = binaryIndexKeyValuePair;
		if ( binaryIndex?.FileName == null )
		{
			return null;
		}

		if ( _hostFileSystemStorage.ExistFile(GetExePath(currentArchitecture)) )
		{
			return true;
		}

		var zipFullFilePath =
			Path.Combine(_appSettings.DependenciesFolder, binaryIndex.FileName);

		if ( !await DownloadMirror(baseUrls, zipFullFilePath, binaryIndex, retryInSeconds) )
		{
			_logger.LogError("Download failed");
			return null;
		}

		if ( !new CheckSha256Helper(_hostFileSystemStorage).CheckSha256(zipFullFilePath,
			    [binaryIndex.Sha256]) )
		{
			_logger.LogError("Sha256 check failed");
			return null;
		}

		new Zipper(_logger).ExtractZip(zipFullFilePath,
			Path.Combine(_appSettings.DependenciesFolder, FfmpegDependenciesFolder));

		if ( !_hostFileSystemStorage.ExistFile(GetExePath(currentArchitecture)) )
		{
			return false;
		}

		_hostFileSystemStorage.FileDelete(zipFullFilePath);
		return true;
	}

	private string GetExePath(string currentArchitecture)
	{
		var exeFile = Path.Combine(_appSettings.DependenciesFolder, FfmpegDependenciesFolder,
			"ffmpeg");
		if ( currentArchitecture is "win-x64" or "win-arm64" )
		{
			exeFile += ".exe";
		}

		return exeFile;
	}

	private async Task<bool> PrepareBeforeRunning(string currentArchitecture)
	{
		var exeFile = GetExePath(currentArchitecture);

		if ( !_hostFileSystemStorage.ExistFile(Path.Combine(exeFile)) )
		{
			return false;
		}

		if ( currentArchitecture is "win-arm64" or "win-x64" )
		{
			return true;
		}

		if ( !await Chmod(exeFile) )
		{
			return false;
		}

		if ( currentArchitecture is "osx-x64" or "osx-arm64" )
		{
			return await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);
		}

		return true;
	}

	private async Task<bool> Chmod(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile("/bin/chmod") )
		{
			_logger.LogError("[RunChmodOnFfmpegExe] WARNING: /bin/chmod does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run("/bin/chmod", "0755", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}


	private async Task<bool> DownloadMirror(List<Uri> baseUrls, string zipFullFilePath,
		BinaryIndex binaryIndex, int retryInSeconds = 15)
	{
		foreach ( var uri in baseUrls.Select(baseUrl => new Uri(baseUrl + binaryIndex.FileName)) )
		{
			if ( await _httpClientHelper.Download(uri, zipFullFilePath, retryInSeconds) )
			{
				return true;
			}
		}

		return false;
	}

	private void CreateDirectoryDependenciesFolderIfNotExists()
	{
		foreach ( var path in new List<string>
		         {
			         _appSettings.DependenciesFolder,
			         Path.Combine(_appSettings.DependenciesFolder, FfmpegDependenciesFolder)
		         } )
		{
			if ( _hostFileSystemStorage.ExistFolder(path) )
			{
				continue;
			}

			_logger.LogInformation("[FfMpegDownload] Create Directory: " +
			                       path);
			_hostFileSystemStorage.CreateDirectory(path);
		}
	}
}
