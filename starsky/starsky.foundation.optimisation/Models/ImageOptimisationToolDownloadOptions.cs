namespace starsky.foundation.optimisation.Models;

public class ImageOptimisationToolDownloadOptions
{
	public required string ToolName { get; init; }
	public required List<Uri> IndexUrls { get; init; }
	public required List<Uri> BaseUrls { get; init; }
	public bool RunChmodOnUnix { get; init; } = true;
}
