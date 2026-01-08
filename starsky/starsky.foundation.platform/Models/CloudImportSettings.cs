using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.platform.Models;

/// <summary>
///     Configuration for Cloud Import providers
/// </summary>
public class CloudImportSettings
{
	/// <summary>
	///     List of Cloud Import provider configurations
	/// </summary>
	public List<CloudImportProviderSettings> Providers { get; set; } = new();

	public List<CloudImportProviderSettings> GetEnabledProviders()
	{
		return Providers.Where(p => p.Enabled &&
		                            ( p.SyncFrequencyHours > 0 ||
		                              p.SyncFrequencyMinutes > 0 ))
			.ToList();
	}
}

/// <summary>
///     Configuration for a single Cloud Import provider
/// </summary>
public class CloudImportProviderSettings
{
	/// <summary>
	///     Unique identifier for this provider configuration
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///     Enable or disable this Cloud Import provider
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	///     Cloud storage provider (e.g., "Dropbox")
	/// </summary>
	public string Provider { get; set; } = "Dropbox";

	/// <summary>
	///     Remote folder path to sync from
	/// </summary>
	public string RemoteFolder { get; set; } = "/";

	/// <summary>
	///     Sync frequency in minutes (if > 0, this takes priority)
	/// </summary>
	public double SyncFrequencyMinutes { get; set; }

	/// <summary>
	///     Sync frequency in hours (used if SyncFrequencyMinutes is 0)
	/// </summary>
	public int SyncFrequencyHours { get; set; }

	/// <summary>
	///     Whether to delete files from cloud storage after successful import
	/// </summary>
	public bool DeleteAfterImport { get; set; }

	/// <summary>
	///     If used, only files with these extensions will be imported
	///     (Does NOT check actual imageFormat)
	///     If empty or null, all files are imported
	/// </summary>
	public List<string> Extensions { get; set; } = [];

	/// <summary>
	///     Credentials for the cloud provider
	/// </summary>
	public CloudProviderCredentials Credentials { get; set; } = new();
}

public class CloudProviderCredentials
{
	public string RefreshToken { get; set; } = string.Empty;
	public string AppKey { get; set; } = string.Empty;
	public string AppSecret { get; set; } = string.Empty;
}
