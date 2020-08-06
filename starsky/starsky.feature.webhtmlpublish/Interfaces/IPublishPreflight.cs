using System.Collections.Generic;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IPublishPreflight
	{
		string GetNameConsole(string inputPath, IReadOnlyList<string> args);
		List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName);
	}
}
