using System.Globalization;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.geo.GeoRegionInfo;

public class RegionInfoHelper(IWebLogger logger)
{
	public Tuple<string, string> GetLocationCountryAndCode(string countryCode)
	{
		// Catch is used for example the region TA
		var locationCountry = string.Empty;
		var locationCountryCode = string.Empty;
		try
		{
			var region = new RegionInfo(countryCode);
			locationCountry = region.EnglishName;
			locationCountryCode = region.ThreeLetterISORegionName;
		}
		catch ( ArgumentException e )
		{
			logger.LogInformation("[GetLocationCountryAndCode] " + e.Message);
		}

		return new Tuple<string, string>(locationCountry, locationCountryCode);
	}
}
