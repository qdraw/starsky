using System.Text.Json;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfMpegDownload : IFfMpegDownload
{
	private readonly AppSettings _appSettings;

	private readonly Uri _ffMpegApiIndex =
		new("https://starsky-dependencies.netlify.app/ffmpeg/index.json");

	private readonly Uri _ffMpegApiIndexMirror =
		new("https://qdraw.nl/special/mirror/ffmpeg/index.json");

	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly Uri FFMpegApiBasePath = new("https://starsky-dependencies.netlify.app/ffmpeg");

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

		var index = await DownloadIndex();
		await GetUrlFromIndex(index);

		return true;
	}

	private async Task<BinaryIndex?> GetUrlFromIndex(FfmpegBinariesIndex? index)
	{
		return index?.Binaries.Find(p => p.Architecture == "win64");
	}

	private async Task<FfmpegBinariesIndex?> DownloadIndex()
	{
		var result = await _httpClientHelper.ReadString(_ffMpegApiIndex);
		if ( result.Key )
		{
			return JsonSerializer.Deserialize<FfmpegBinariesIndex>(result.Value,
				DefaultJsonSerializer.CamelCase);
		}

		result = await _httpClientHelper.ReadString(_ffMpegApiIndexMirror);
		if ( result.Key )
		{
			return JsonSerializer.Deserialize<FfmpegBinariesIndex>(result.Value,
				DefaultJsonSerializer.CamelCase);
		}

		_logger.LogError("[FfMpegDownload] Index not found");
		return new FfmpegBinariesIndex { Success = false };
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
