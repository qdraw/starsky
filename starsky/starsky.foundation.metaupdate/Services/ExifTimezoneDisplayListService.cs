using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Services;

[Service(typeof(IExifTimezoneDisplayListService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ExifTimezoneDisplayListService : IExifTimezoneDisplayListService
{
	public List<ExifTimezoneDisplay> GetIncorrectCameraTimezonesList()
	{
		var timezones = new List<ExifTimezoneDisplay>();

		for ( var i = 12; i >= -14; i-- )
		{
			if ( i == 0 )
			{
				timezones.Add(new ExifTimezoneDisplay { Id = "Etc/GMT", DisplayName = "UTC" });
				continue;
			}

			// Construct identifier per IANA naming
			var gmtId = i > 0 ? $"Etc/GMT+{i}" : $"Etc/GMT{i}";

			// Invert sign for display: + in id means negative UTC offset
			var hours = Math.Abs(i);
			var sign = i > 0 ? "-" : "+";
			var displayName = $"UTC{sign}{hours:D2}";

			timezones.Add(new ExifTimezoneDisplay { Id = gmtId, DisplayName = displayName });
		}

		return timezones;
	}

	public List<ExifTimezoneDisplay> GetMovedToDifferentPlaceTimezonesList()
	{
		// Add standard system timezones
		// Add Etc/GMT timezones (fixed offset, no DST)
		// GMT offset format: Etc/GMT+X where X is hours behind UTC (opposite sign)

		var timezones =
			TimeZoneInfo.GetSystemTimeZones()
				.Select(tz => new ExifTimezoneDisplay { Id = tz.Id, DisplayName = tz.DisplayName })
				.ToList();

		return timezones;
	}
}
