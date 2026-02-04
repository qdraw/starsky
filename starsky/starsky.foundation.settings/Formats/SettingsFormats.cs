using System;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.settings.Formats;

public static class SettingsFormats
{
	internal const string DefaultSettingsDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

	public static string ToDefaultSettingsFormat(this DateTime dateTime)
	{
		return dateTime.ToString(DefaultSettingsDateTimeFormat, CultureInfo.InvariantCulture);
	}
}
