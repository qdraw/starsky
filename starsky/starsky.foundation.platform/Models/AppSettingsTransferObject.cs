namespace starsky.foundation.platform.Models
{
	/// <summary>
	/// Used to update AppSettings from the UI 'preferences'
	/// </summary>
	public class AppSettingsTransferObject
	{
		public bool? Verbose { get; set; }
		public string StorageFolder { get; set; }
	}
}
