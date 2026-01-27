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

	public List<ExifTimezoneDisplay> GetMovedToDifferentPlaceTimezonesList(DateTime? dateTime)
	{
		dateTime ??= DateTime.UtcNow;

		// Get all system timezones with their offset calculated for the specific date
		// This ensures DST is shown correctly: summer = DST offset, winter = standard offset
		var timezones = TimeZoneInfo.GetSystemTimeZones()
			.Select(tz =>
			{
				// Get the UTC offset for this specific datetime (DST-aware)
				var offset = tz.GetUtcOffset(dateTime.Value);

				// Format offset as +/-HH:mm
				var sign = offset < TimeSpan.Zero ? "-" : "+";
				var absOffset = offset.Duration();
				var offsetString = $"{sign}{absOffset.Hours:D2}:{absOffset.Minutes:D2}";

				// Create display name with actual offset for this date
				var displayName = $"(UTC{offsetString}) {tz.StandardName}";

				return new ExifTimezoneDisplay { Id = tz.Id, DisplayName = displayName };
			})
			.DistinctBy(tz => tz.DisplayName).ToList();

		return timezones;
	}
}
