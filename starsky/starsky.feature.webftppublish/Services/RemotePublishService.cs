using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webftppublish.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.webftppublish.Services;

/// <summary>
///     Composite service that delegates to FTP or LocalFileSystem based on configuration
/// </summary>
[Service(typeof(IRemotePublishService), InjectionLifetime = InjectionLifetime.Scoped)]
public class RemotePublishService(
	IFtpService ftpService,
	ILocalFileSystemPublishService localFileSystemService,
	AppSettings appSettings,
	IWebLogger logger)
	: IRemotePublishService
{
	public async Task<FtpPublishManifestModel?> IsValidZipOrFolder(
		string inputFullFileDirectoryOrZip)
	{
		// Validation is same for all types, use FTP service
		return await ftpService.IsValidZipOrFolder(inputFullFileDirectoryOrZip);
	}

	public bool Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent)
	{
		var publishItems = 
			appSettings.PublishProfilesRemote.GetById(profileId);
		if ( publishItems.Count == 0 )
		{
			return false;
		}

		var success = false;
		foreach ( var publishItemType in publishItems.Select(p => p.Type) )
		{
			switch ( publishItemType )
			{
				case RemoteCredentialType.Ftp:
					if ( ftpService.Run(parentDirectoryOrZipFile, profileId, slug,
						    copyContent) )
					{
						success = true;
					}

					break;
				case RemoteCredentialType.LocalFileSystem:
					if ( localFileSystemService.Run(parentDirectoryOrZipFile, profileId, slug,
						    copyContent) )
					{
						success = true;
					}

					break;
				default:
					logger.LogError($"Unsupported remote credential type: {publishItemType}");
					break;
			}
		}

		return success;
	}

	public bool IsPublishEnabled(string publishProfileName)
	{
		if ( appSettings.PublishProfilesRemote.GetById(publishProfileName).Count == 0 )
		{
			return false;
		}

		var profile = appSettings.PublishProfiles!
			.FirstOrDefault(p => p.Key == publishProfileName);

		if ( profile.Key == null || profile.Value == null || profile.Value.Count == 0 )
		{
			logger.LogInformation("Profile is missing or empty, skipping publish profile check");
			return false;
		}

		var isPublish = profile.Value.Any(p => p.ContentType == TemplateContentType.PublishRemote);

		return isPublish;
	}
}
