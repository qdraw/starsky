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
public class FfMpegDownloadBinaries(
	ISelectorStorage selectorStorage,
	IHttpClientHelper httpClientHelper,
	AppSettings appSettings,
	IWebLogger logger,
	IZipper zipper)
	: IFfMpegDownloadBinaries
{
	private readonly FfmpegExePath _ffmpegExePath = new(appSettings);
	private readonly IStorage _hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public async Task<FfmpegDownloadStatus> Download(
		KeyValuePair<BinaryIndex?, List<Uri>> binaryIndexKeyValuePair, string currentArchitecture,
		int retryInSeconds = 15)
	{
		var (binaryIndex, baseUrls) = binaryIndexKeyValuePair;

		if ( binaryIndex?.FileName == null )
		{
			return FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName;
		}

		var exePath = _ffmpegExePath.GetExePath(currentArchitecture);

		if ( _hostFileSystemStorage.ExistFile(exePath) )
		{
			return FfmpegDownloadStatus.OkAlreadyExists;
		}

		var zipFullFilePath =
			Path.Combine(appSettings.DependenciesFolder, binaryIndex.FileName);

		if ( !await DownloadMirror(baseUrls, zipFullFilePath, binaryIndex, retryInSeconds) )
		{
			logger.LogError("[FfMpegDownloadBinaries] Download failed");
			return FfmpegDownloadStatus.DownloadBinariesFailed;
		}

		if ( !new CheckSha256Helper(_hostFileSystemStorage).CheckSha256(zipFullFilePath,
			    [binaryIndex.Sha256]) )
		{
			logger.LogError("[FfMpegDownloadBinaries] Sha256 check failed");
			return FfmpegDownloadStatus.DownloadBinariesFailedSha256Check;
		}

		zipper.ExtractZip(zipFullFilePath, _ffmpegExePath.GetExeParentFolder(currentArchitecture));

		if ( !_hostFileSystemStorage.ExistFile(exePath) )
		{
			logger.LogError($"[FfMpegDownloadBinaries] Zipper failed {exePath}");
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
			if ( await httpClientHelper.Download(uri, zipFullFilePath, retryInSeconds) )
			{
				return true;
			}
		}

		return false;
	}
}
