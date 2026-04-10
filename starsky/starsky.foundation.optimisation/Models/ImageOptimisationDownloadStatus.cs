namespace starsky.foundation.optimisation.Models;

public enum ImageOptimisationDownloadStatus
{
	Ok = 0,
	OkAlreadyDownloaded = 1,
	SettingsDisabled = 2,
	DownloadIndexFailed = 3,
	DownloadBinariesFailedMissingFileName = 4,
	DownloadBinariesFailed = 5,
	DownloadBinariesFailedSha256Check = 6,
	DownloadBinariesFailedZipperNotExtracted = 7,
	RunChmodFailed = 8
}
