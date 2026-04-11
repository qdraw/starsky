namespace starsky.foundation.platform.Models;

/// <summary>
/// 	Tenant configuration entry.
/// </summary>
public sealed class Tenant
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public StorageProvider Storage { get; set; } = new();
}

