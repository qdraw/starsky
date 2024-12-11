using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegDownloadBinaries), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegDownloadBinaries : IFfMpegDownloadBinaries
{
	private readonly AppSettings _appSettings;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IWebLogger _logger;
	private readonly IZipper _zipper;

	public FfMpegDownloadBinaries(ISelectorStorage selectorStorage,
		IHttpClientHelper httpClientHelper, AppSettings appSettings, IWebLogger logger,
		IZipper zipper)
	{
		_appSettings = appSettings;
		_httpClientHelper = httpClientHelper;
		_logger = logger;
		_ffmpegExePath = new FfmpegExePath(appSettings);
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_zipper = zipper;
	}

	public async Task<FfmpegDownloadStatus> Download(
		KeyValuePair<BinaryIndex?, List<Uri>> binaryIndexKeyValuePair, string currentArchitecture,
		int retryInSeconds = 15)
	{
		var (binaryIndex, baseUrls) = binaryIndexKeyValuePair;
		if ( binaryIndex?.FileName == null )
		{
			return FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName;
		}

		if ( _hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(currentArchitecture)) )
		{
			return FfmpegDownloadStatus.Ok;
		}

		var zipFullFilePath =
			Path.Combine(_appSettings.DependenciesFolder, binaryIndex.FileName);

		if ( !await DownloadMirror(baseUrls, zipFullFilePath, binaryIndex, retryInSeconds) )
		{
			_logger.LogError("Download failed");
			return FfmpegDownloadStatus.DownloadBinariesFailed;
		}

		if ( !new CheckSha256Helper(_hostFileSystemStorage).CheckSha256(zipFullFilePath,
			    [binaryIndex.Sha256]) )
		{
			_logger.LogError("Sha256 check failed");
			return FfmpegDownloadStatus.DownloadBinariesFailedSha256Check;
		}

		_zipper.ExtractZip(zipFullFilePath, _ffmpegExePath.GetExeParentFolder());

		if ( !_hostFileSystemStorage.ExistFile(_ffmpegExePath.GetExePath(currentArchitecture)) )
		{
			_logger.LogError($"Zipper failed {_ffmpegExePath.GetExePath(currentArchitecture)}");
			return FfmpegDownloadStatus.DownloadBinariesFailedZipperNotExtracted;
		}

		_hostFileSystemStorage.FileDelete(zipFullFilePath);

		return FfmpegDownloadStatus.Ok;
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
}
