using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static partial class ValidateLocation
{
	/// <summary>
	///     un-escaped: ^[+-]?(([1-8]?[0-9])(\.[0-9]{1,6})?|90(\.0{1,6})?)$
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex("^[+-]?(([1-8]?[0-9])(\\.[0-9]{1,6})?|90(\\.0{1,6})?)$", RegexOptions.None,
		1000)]
	private static partial Regex LatitudeRegex();

	/// <summary>
	///     un-escaped ^[+-]?((([1-9]?[0-9]|1[0-7][0-9])(\.[0-9]{1,6})?)|180(\.0{1,6})?)$
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex("^[+-]?((([1-9]?[0-9]|1[0-7][0-9])(\\.[0-9]{1,6})?)|180(\\.0{1,6})?)$",
		RegexOptions.None, 1000)]
	private static partial Regex LongitudeRegex();

	public static bool ValidateLatitudeLongitude(double latitude, double longitude)
	{
		var latitudeValue = Math.Round(latitude, 6).ToString(CultureInfo.InvariantCulture);
		var longitudeValue = Math.Round(longitude, 6).ToString(CultureInfo.InvariantCulture);

		return LatitudeRegex().IsMatch(latitudeValue) &&
		       LongitudeRegex().IsMatch(longitudeValue);
	}
}
