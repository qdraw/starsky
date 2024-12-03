using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfMpegDownload : IFfMpegDownload
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;

	public FfMpegDownload(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
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

		var container = await new FfMpegDownloadIndex(_httpClientHelper, _logger).DownloadIndex();
		var binaryIndexBaseUrls = GetCurrentArchitectureIndexUrls(container,
			currentArchitecture);

		await Download(binaryIndexBaseUrls);
		
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

	private async Task Download(KeyValuePair<BinaryIndex, List<Uri>> binaryIndexKeyValuePair)
	{
		var ( binaryIndex, baseUrls ) = binaryIndexKeyValuePair;

		if ( !_hostFileSystemStorage.ExistFile(
			    Path.Combine(_appSettings.DependenciesFolder, binaryIndex.Url)) )
		{

		foreach ( var baseUrl in baseUrls )
		{

			
			var uri = new Uri(baseUrl + binaryIndex.Url);
			
		}
		
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
