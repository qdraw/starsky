using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Models;

namespace starsky.feature.webftppublish.Interfaces;

/// <summary>
///     Selector service that delegates to the appropriate publish implementation
/// </summary>
public interface IRemotePublishService
{
	/// <summary>
	///     Validates if the input is a valid zip file or folder with manifest
	/// </summary>
	/// <param name="inputFullFileDirectoryOrZip">Path to zip or folder</param>
	/// <returns>Manifest model or null if invalid</returns>
	Task<FtpPublishManifestModel?> IsValidZipOrFolder(string inputFullFileDirectoryOrZip);

	/// <summary>
	///     Publishes content to the configured destination based on profile configuration
	/// </summary>
	/// <param name="parentDirectoryOrZipFile">Source directory or zip file</param>
	/// <param name="profileId">Profile ID for configuration lookup</param>
	/// <param name="slug">Slug/name for the published content</param>
	/// <param name="copyContent">Dictionary of files to copy</param>
	/// <returns>True if successful</returns>
	bool Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent);

	bool IsPublishEnabled(string publishProfileName);
}
