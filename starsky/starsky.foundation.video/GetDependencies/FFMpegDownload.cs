using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

public class FfMpegDownload : IFfMpegDownload
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly Uri FFMpegApiBasePath = new("https://starsky-dependencies.netlify.app/ffmpeg");

	private readonly Uri _ffMpegApiIndex =
		new("https://starsky-dependencies.netlify.app/ffmpeg/index.json");
	private readonly Uri _ffMpegApiIndex =
		new("https://qdraw.nl/special/mirror/ffmpeg/index.json");
	public FfMpegDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
	}

	public async Task<bool> DownloadFFMpeg()
	{
		if ( _appSettings.ExiftoolSkipDownloadOnStartup == true || _appSettings is
			    { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
		{
			var name = _appSettings.FfmpegSkipDownloadOnStartup == true
				? "FFMpegSkipDownloadOnStartup"
				: "AddSwaggerExport and AddSwaggerExportExitAfter";
			_logger.LogInformation($"[DownloadFFMpeg] Skipped due true of {name} setting");
			return false;
		}

		CreateDirectoryDependenciesFolderIfNotExists();

		return true;
	}

	private async Task DownloadIndex()
	{
		var result = await _httpClientHelper.ReadString(_ffMpegApiIndex);
		if ( !result.Key )
		{
			_logger.LogError("[FfMpegDownload] Index not found");
			return;
		}
	}

	private async Task Download(Uri FFMpegDownloadBasePath)
	{
		// if ( !_hostFileSystemStorage.ExistFile(
		// 	    Path.Combine(_appSettings.DependenciesFolder, CountryName + ".txt")) )
		// {
		// 	var outputZip = Path.Combine(_appSettings.DependenciesFolder,
		// 		CountryName + ".zip");
		// 	var baseResult =
		// 		await _httpClientHelper.Download(https + BaseUrl + CountryName + ".zip",
		// 			outputZip);
		// 	if ( !baseResult )
		// 	{
		// 		await _httpClientHelper.Download(https + MirrorUrl + CountryName + ".zip",
		// 			outputZip);
		// 	}
		//
		// 	new Zipper(_logger).ExtractZip(outputZip, _appSettings.DependenciesFolder);
		// }
	}

	private void CreateDirectoryDependenciesFolderIfNotExists()
	{
		if ( _hostFileSystemStorage.ExistFolder(
			    _appSettings.DependenciesFolder) )
		{
			return;
		}

		_logger.LogInformation("[FfMpegDownload] Create Directory: " +
		                       _appSettings.DependenciesFolder);
		_hostFileSystemStorage.CreateDirectory(_appSettings.DependenciesFolder);
	}
}
