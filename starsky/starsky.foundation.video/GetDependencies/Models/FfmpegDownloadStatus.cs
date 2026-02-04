namespace starsky.foundation.video.GetDependencies.Models;

public enum FfmpegDownloadStatus
{
	Ok,
	OkAlreadyExists,
	SettingsDisabled,
	DownloadIndexFailed,
	DownloadBinariesFailed,
	DownloadBinariesFailedMissingFileName,
	DownloadBinariesFailedSha256Check,
	DownloadBinariesFailedZipperNotExtracted,
	PrepareBeforeRunningFailed,
	PreflightRunCheckFailed
}
