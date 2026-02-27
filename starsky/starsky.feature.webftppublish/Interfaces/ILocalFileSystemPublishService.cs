using System.Collections.Generic;

namespace starsky.feature.webftppublish.Interfaces;

public interface ILocalFileSystemPublishService
{
	/// <summary>
	///     Publishes content to the configured destination
	/// </summary>
	/// <param name="parentDirectoryOrZipFile">Source directory or zip file</param>
	/// <param name="profileId">Profile ID for configuration lookup</param>
	/// <param name="slug">Slug/name for the published content</param>
	/// <param name="copyContent">Dictionary of files to copy</param>
	/// <returns>True if successful</returns>
	bool Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent);
}
