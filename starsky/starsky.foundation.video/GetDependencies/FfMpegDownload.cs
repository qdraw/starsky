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
	private readonly IFfMpegPreflightRunCheck _preflightRunCheck;
	private readonly IFfMpegPrepareBeforeRunning _prepareBeforeRunning;

	public FfMpegDownload(ISelectorStorage selectorStorage,
		AppSettings appSettings,
		IWebLogger logger, IFfMpegDownloadIndex downloadIndex,
		IFfMpegDownloadBinaries downloadBinaries, IFfMpegPrepareBeforeRunning prepareBeforeRunning,
		IFfMpegPreflightRunCheck preflightRunCheck)
	{
		_appSettings = appSettings;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
		_downloadIndex = downloadIndex;
		_ffmpegExePath = new FfmpegExePath(_appSettings);
		_downloadBinaries = downloadBinaries;
		_prepareBeforeRunning = prepareBeforeRunning;
		_preflightRunCheck = preflightRunCheck;
	}

	public async Task<List<FfmpegDownloadStatus>> DownloadFfMpeg(List<string> architectures)
	{
		if ( architectures.Count == 0 )
		{
			architectures.Add(CurrentArchitecture.GetCurrentRuntimeIdentifier());
		}

		var result = new List<FfmpegDownloadStatus>();
		foreach ( var architecture in architectures.Where(p => p !=
		                                                       DotnetRuntimeNames
			                                                       .GenericRuntimeName) )
		{
			result.Add(await DownloadFfMpeg(architecture));
		}

		return result;
	}

	public async Task<FfmpegDownloadStatus> DownloadFfMpeg(string? architecture = null)
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

		architecture ??= CurrentArchitecture.GetCurrentRuntimeIdentifier();
		CreateDirectoryDependenciesFolderIfNotExists(architecture);

		if ( _hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(architecture)) )
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
			architecture);

		var downloadStatus =
			await _downloadBinaries.Download(binaryIndexBaseUrls, architecture);
		if ( downloadStatus != FfmpegDownloadStatus.Ok &&
		     downloadStatus != FfmpegDownloadStatus.OkAlreadyExists )
		{
			_logger.LogError($"[FfMpegDownload] Binaries downloading failed {downloadStatus}");
			return downloadStatus;
		}

		if ( !await _prepareBeforeRunning.PrepareBeforeRunning(architecture) )
		{
			return FfmpegDownloadStatus.PrepareBeforeRunningFailed;
		}

		if ( !await _preflightRunCheck.TryRun(architecture) )
		{
			return FfmpegDownloadStatus.PreflightRunCheckFailed;
		}

		return FfmpegDownloadStatus.Ok;
	}

	public string GetSetFfMpegPath()
	{
		var path = _ffmpegExePath.GetExePath(CurrentArchitecture.GetCurrentRuntimeIdentifier());
		if ( _appSettings.FfmpegPath != null &&
		     _hostFileSystemStorage.ExistFile(_appSettings.FfmpegPath) )
		{
			return _appSettings.FfmpegPath;
		}

		_appSettings.FfmpegPath = path;
		return path;
	}


	private static KeyValuePair<BinaryIndex?, List<Uri>> GetCurrentArchitectureIndexUrls(
		FfmpegBinariesContainer container,
		string currentArchitecture)
	{
		var sortedData = container.Data?.Binaries.Find(p =>
			p.Architecture == currentArchitecture);

		return new KeyValuePair<BinaryIndex?, List<Uri>>(sortedData, container.BaseUrls);
	}

	private void CreateDirectoryDependenciesFolderIfNotExists(string currentArchitecture)
	{
		foreach ( var path in new List<string>
		         {
			         _appSettings.DependenciesFolder,
			         _ffmpegExePath.GetExeParentFolder(currentArchitecture)
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
