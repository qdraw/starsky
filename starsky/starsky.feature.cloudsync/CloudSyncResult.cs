namespace starsky.foundation.cloudsync;

public class CloudSyncResult
{
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public CloudSyncTriggerType TriggerType { get; set; }
	public int FilesFound { get; set; }
	public int FilesImportedSuccessfully { get; set; }
	public int FilesSkipped { get; set; }
	public int FilesFailed { get; set; }
	public List<string> Errors { get; set; } = new();
	public List<string> SuccessfulFiles { get; set; } = new();
	public List<string> FailedFiles { get; set; } = new();
	public bool Success => FilesFailed == 0 && Errors.Count == 0;
}

public enum CloudSyncTriggerType
{
	Scheduled,
	Manual
}
