using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegDownload), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegDownload : IFfMpegDownload
{
	private readonly AppSettings _appSettings;
	private readonly IFfMpegDownloadIndex _downloadIndex;
	private readonly FfmpegChmod _ffmpegChmod;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly IMacCodeSign _macCodeSign;

	public FfMpegDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger, IMacCodeSign macCodeSign, IFfMpegDownloadIndex downloadIndex)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
		_macCodeSign = macCodeSign;
		_downloadIndex = downloadIndex;
		_ffmpegExePath = new FfmpegExePath(_appSettings);
		_ffmpegChmod = new FfmpegChmod(_hostFileSystemStorage, _logger);
	}

	public async Task<FfmpegDownloadStatus> DownloadFfMpeg()
	{
		if ( _appSettings.FfmpegSkipDownloadOnStartup == true
		     || _appSettings is { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
		{
			var name = _appSettings.FfmpegSkipDownloadOnStartup == true
				? "FFMpegSkipDownloadOnStartup"
				: "AddSwaggerExport and AddSwaggerExportExitAfter";
			_logger.LogInformation($"[DownloadFFMpeg] Skipped due true of {name} setting");
			return FfmpegDownloadStatus.SettingsDisabled;
		}

		CreateDirectoryDependenciesFolderIfNotExists();

		var currentArchitecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();

		if ( _hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(currentArchitecture)) )
		{
			return FfmpegDownloadStatus.Ok;
		}

		var container = await _downloadIndex.DownloadIndex();
		if ( !container.Success )
		{
			_logger.LogError("[FfMpegDownload] Index not found");
			return FfmpegDownloadStatus.DownloadIndexFailed;
		}

		var binaryIndexBaseUrls = GetCurrentArchitectureIndexUrls(container,
			currentArchitecture);

		var download = await Download(binaryIndexBaseUrls, currentArchitecture);
		if ( download is null or false )
		{
			_logger.LogError("[FfMpegDownload] Binaries not found");
			return FfmpegDownloadStatus.DownloadBinariesFailed;
		}

		if ( !await PrepareBeforeRunning(currentArchitecture) )
		{
			return FfmpegDownloadStatus.PrepareBeforeRunningFailed;
		}

		return FfmpegDownloadStatus.Ok;
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

		if ( _hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(currentArchitecture)) )
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

		new Zipper(_logger).ExtractZip(zipFullFilePath, _ffmpegExePath.GetExeParentFolder());

		if ( !_hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(currentArchitecture)) )
		{
			return false;
		}

		_hostFileSystemStorage.FileDelete(zipFullFilePath);
		return true;
	}


	private async Task<bool> PrepareBeforeRunning(string currentArchitecture)
	{
		var exeFile = _ffmpegExePath.GetExePath(currentArchitecture);

		if ( !_hostFileSystemStorage.ExistFile(Path.Combine(exeFile)) )
		{
			return false;
		}

		if ( currentArchitecture is "win-arm64" or "win-x64" )
		{
			return true;
		}

		if ( !await _ffmpegChmod.Chmod(exeFile) )
		{
			return false;
		}

		if ( currentArchitecture is "osx-x64" or "osx-arm64" )
		{
			return await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile);
		}

		return true;
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
			         _appSettings.DependenciesFolder, _ffmpegExePath.GetExeParentFolder()
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
