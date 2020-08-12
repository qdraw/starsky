using System.Collections.Generic;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IPublishPreflight
	{
		IEnumerable<string> GetAllPublishProfileNames();
		string GetNameConsole(string inputPath, IReadOnlyList<string> args);
		List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName);
	}
}
