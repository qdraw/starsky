namespace starsky.foundation.optimisation.Models;

public enum ImageOptimisationDownloadStatus
{
	Ok = 0,
	OkAlreadyDownloaded = 1,
	DownloadIndexFailed = 2,
	DownloadBinariesFailedMissingFileName = 3,
	DownloadBinariesFailed = 4,
	DownloadBinariesFailedSha256Check = 5,
	DownloadBinariesFailedZipperNotExtracted = 6,
	RunChmodFailed = 7
}
