using System.Globalization;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.geo.GeoRegionInfo;

public class RegionInfoHelper(IWebLogger logger)
{
	public Tuple<string, string> GetLocationCountryAndCode(string countryCode)
	{
		// Catch is used for example the region TA
		var englishCountryName = string.Empty;
		var threeLetterLocationCountry = string.Empty;
		try
		{
			var region = new RegionInfo(countryCode);
			englishCountryName = region.EnglishName;
			threeLetterLocationCountry = region.ThreeLetterISORegionName;
		}
		catch ( ArgumentException e )
		{
			logger.LogInformation("[GetLocationCountryAndCode] " + e.Message);
		}

		return new Tuple<string, string>(englishCountryName, threeLetterLocationCountry);
	}
}
