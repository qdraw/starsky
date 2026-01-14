namespace starsky.feature.cloudimport;

public class CloudFile
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Path { get; set; } = string.Empty;
	public long Size { get; set; }
	public DateTime? ModifiedDate { get; set; }
	public string Hash { get; set; } = string.Empty;
}
