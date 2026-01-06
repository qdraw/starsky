namespace starsky.foundation.cloudsync;

public class CloudSyncResult
{
	public string ProviderId { get; set; } = string.Empty;
	public string ProviderName { get; set; } = string.Empty;
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public CloudSyncTriggerType TriggerType { get; set; }
	public int FilesFound { get; set; }
	public int FilesImportedSuccessfully { get; set; }
	public int FilesSkipped { get; set; }
	public int FilesFailed { get; set; }
	public List<string> Errors { get; set; } = [];
	public List<string> SuccessfulFiles { get; set; } = [];
	public List<string> FailedFiles { get; set; } = [];
	public bool Success => FilesFailed == 0 && Errors.Count == 0;

	public bool? SkippedNoInput { get; set; } = false;
}

public enum CloudSyncTriggerType
{
	Scheduled,
	Manual,
	CommandLineInterface
}
