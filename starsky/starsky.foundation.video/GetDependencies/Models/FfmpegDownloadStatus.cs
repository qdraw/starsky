namespace starsky.foundation.video.GetDependencies.Models;

public enum FfmpegDownloadStatus
{
	Ok,
	SettingsDisabled,
	DownloadIndexFailed,
	DownloadBinariesFailed,
	DownloadBinariesFailedMissingFileName,
	DownloadBinariesFailedSha256Check,
	DownloadBinariesFailedZipperNotExtracted,
	PrepareBeforeRunningFailed
}
