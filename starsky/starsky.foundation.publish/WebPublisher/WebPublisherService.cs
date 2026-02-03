using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.publish.WebPublisher.Interfaces;

namespace starsky.foundation.publish.WebPublisher;

[Service(typeof(IWebPublisherService), InjectionLifetime = InjectionLifetime.Scoped)]
public class WebPublisherService(AppSettings appSettings) : IWebPublisherService
{
	/// <summary>
	///     Check if the profile is allowed to publish to FTP
	/// </summary>
	/// <param name="publishProfileName">profile key</param>
	/// <returns>true if profile can be published, false otherwise</returns>
	public bool IsProfilePublishable(string publishProfileName)
	{
		if ( string.IsNullOrEmpty(publishProfileName) )
		{
			return false;
		}

		var profile = appSettings.PublishProfiles!
			.FirstOrDefault(p => p.Key == publishProfileName);

		if ( profile.Key == null || profile.Value == null || profile.Value.Count == 0 )
		{
			return false;
		}

		// TODO: not sure if this correct? all items need to be webpublish?
		// Check if all items in the profile have WebPublish enabled
		return profile.Value.All(p => p.WebPublish);
	}
}
