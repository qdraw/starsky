namespace starsky.foundation.metaupdate.Models;

public class ExifTimezoneDisplay
{
	public required string Id { get; set; }
	public required string DisplayName { get; set; }
	public List<string> Aliases { get; set; } = new();
}
