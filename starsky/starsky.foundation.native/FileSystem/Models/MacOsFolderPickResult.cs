namespace starsky.foundation.native.FileSystem.Models;

public class MacOsFolderPickResult
{
	public bool Success { get; set; }
	public string? Path { get; set; }
	public string? BookmarkToken { get; set; }
	public string? Error { get; set; }
}

