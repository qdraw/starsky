namespace starsky.foundation.platform.Models;

/// <summary>
/// 	Storage provider entry for multi-root storage support.
/// </summary>
public sealed class StorageProvider
{
	public string Type { get; set; } = "FileSystem";
	public string Path { get; set; } = string.Empty;
	public string? Token { get; set; }
}

