namespace starsky.foundation.platform.Models;

public class AppSettingsImportBackupModel
{
	public bool Enabled { get; set; } = false;
	public string? StorageFolder { get; set; }
}
