using starsky.foundation.injection;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegDownload), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegDownload : IFfMpegDownload
{
	private readonly AppSettings _appSettings;
	private readonly IFfMpegDownloadBinaries _downloadBinaries;
	private readonly IFfMpegDownloadIndex _downloadIndex;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IFfMpegPrepareBeforeRunning _prepareBeforeRunning;

	public FfMpegDownload(ISelectorStorage selectorStorage,
		AppSettings appSettings,
		IWebLogger logger, IFfMpegDownloadIndex downloadIndex,
		IFfMpegDownloadBinaries downloadBinaries, IFfMpegPrepareBeforeRunning prepareBeforeRunning)
	{
		_appSettings = appSettings;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
		_downloadIndex = downloadIndex;
		_ffmpegExePath = new FfmpegExePath(_appSettings);
		_downloadBinaries = downloadBinaries;
		_prepareBeforeRunning = prepareBeforeRunning;
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

		var downloadStatus =
			await _downloadBinaries.Download(binaryIndexBaseUrls, currentArchitecture);
		if ( downloadStatus != FfmpegDownloadStatus.Ok )
		{
			_logger.LogError($"[FfMpegDownload] Binaries downloading failed {downloadStatus}");
			return downloadStatus;
		}

		if ( !await _prepareBeforeRunning.PrepareBeforeRunning(currentArchitecture) )
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
