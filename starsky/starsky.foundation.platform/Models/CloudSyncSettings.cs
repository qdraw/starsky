using System;
using System.Collections.Generic;

namespace starsky.foundation.platform.Models;

/// <summary>
///     Configuration for cloud sync providers
/// </summary>
public class CloudSyncSettings
{
	/// <summary>
	///     List of cloud sync provider configurations
	/// </summary>
	public List<CloudSyncProviderSettings> Providers { get; set; } = new();
}

/// <summary>
///     Configuration for a single cloud sync provider
/// </summary>
public class CloudSyncProviderSettings
{
	/// <summary>
	///     Unique identifier for this provider configuration
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///     Enable or disable this cloud sync provider
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	///     Cloud storage provider (e.g., "Dropbox", "GoogleDrive", "OneDrive")
	/// </summary>
	public string Provider { get; set; } = "Dropbox";

	/// <summary>
	///     Remote folder path to sync from
	/// </summary>
	public string RemoteFolder { get; set; } = "/";

	/// <summary>
	///     Sync frequency in minutes (if > 0, this takes priority)
	/// </summary>
	public int SyncFrequencyMinutes { get; set; }

	/// <summary>
	///     Sync frequency in hours (used if SyncFrequencyMinutes is 0)
	/// </summary>
	public int SyncFrequencyHours { get; set; } = 24;

	/// <summary>
	///     Whether to delete files from cloud storage after successful import
	/// </summary>
	public bool DeleteAfterImport { get; set; }

	/// <summary>
	///     Credentials for the cloud provider (stored securely)
	/// </summary>
	public CloudProviderCredentials Credentials { get; set; } = new();
}

public class CloudProviderCredentials
{
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public string AppKey { get; set; } = string.Empty;
	public string AppSecret { get; set; } = string.Empty;
	public DateTime? ExpiresAt { get; set; }
}

