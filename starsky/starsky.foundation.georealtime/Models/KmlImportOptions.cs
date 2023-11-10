namespace starsky.foundation.georealtime.Models;

public class KmlImportOptions
{
	public string OutputPath { get; set; }
	public bool OutputStorageSubPath { get; set; } = true;

	public bool SplitByDay { get; set; }
	
}
