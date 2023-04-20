using System;
using System.Collections.Generic;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IPublishPreflight
	{
		/// <summary>
		/// Get all publish profile names
		/// </summary>
		/// <returns>(string: name, bool: isValid)</returns>
		IEnumerable<KeyValuePair<string,bool>> GetAllPublishProfileNames();
		
		/// <summary>
		/// Check if the profile is valid
		/// </summary>
		/// <param name="publishProfileName">profile key</param>
		/// <returns>(bool and list of errors)</returns>
		Tuple<bool,List<string>> IsProfileValid(string publishProfileName);
		
		string GetNameConsole(string inputPath, IReadOnlyList<string> args);
		List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName);
	}
}
