using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static class ValidateLocation
{
	public static bool ValidateLatitudeLongitude(double latitude, double longitude)
	{
		var latitudeValue = Math.Round(latitude, 6).ToString(CultureInfo.InvariantCulture);
		var longitudeValue = Math.Round(longitude, 6).ToString(CultureInfo.InvariantCulture);

		// un-escaped: ^[+-]?(([1-8]?[0-9])(\.[0-9]{1,6})?|90(\.0{1,6})?)$
		var latitudeRegex =
			new Regex(
				"^[+-]?(([1-8]?[0-9])(\\.[0-9]{1,6})?|90(\\.0{1,6})?)$",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));

		// un-escaped ^[+-]?((([1-9]?[0-9]|1[0-7][0-9])(\.[0-9]{1,6})?)|180(\.0{1,6})?)$
		var longitudeRegex =
			new Regex(
				"^[+-]?((([1-9]?[0-9]|1[0-7][0-9])(\\.[0-9]{1,6})?)|180(\\.0{1,6})?)$",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));

		return latitudeRegex.IsMatch(latitudeValue) &&
			   longitudeRegex.IsMatch(longitudeValue);
	}
}


