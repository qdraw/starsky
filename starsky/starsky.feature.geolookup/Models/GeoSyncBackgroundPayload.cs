namespace starsky.feature.geolookup.Models;

public sealed class GeoSyncBackgroundPayload
{
	public string SubPath { get; set; } = "/";
	public bool Index { get; set; }
	public bool OverwriteLocationNames { get; set; }
}
