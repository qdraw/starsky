using System;
using starsky.foundation.platform.Models;

namespace starsky.foundation.import.Models;

public class MountWatcherStatusModel
{
	public bool Enabled { get; set; }
	public bool Running { get; set; }
	public DateTime? LastImportUtc { get; set; }
	public int TotalImportsTriggered { get; set; }
	public string? LastResult { get; set; }
	public AppSettings.MountWatcherImportProcessModel ConfiguredProcessModel { get; set; }
	public string? LastUsedProcessModel { get; set; }
}


