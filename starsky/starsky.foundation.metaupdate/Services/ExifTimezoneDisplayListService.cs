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
				timezones.Add(new ExifTimezoneDisplay
				{
					Id = "Etc/GMT",
					DisplayName = "UTC",
					Aliases = new List<string> { "Etc/GMT" }
				});
				continue;
			}

			// Construct identifier per IANA naming
			var gmtId = i > 0 ? $"Etc/GMT+{i}" : $"Etc/GMT{i}";

			// Invert sign for display: + in id means negative UTC offset
			var hours = Math.Abs(i);
			var sign = i > 0 ? "-" : "+";
			var displayName = $"UTC{sign}{hours:D2}";

			timezones.Add(new ExifTimezoneDisplay
			{
				Id = gmtId, DisplayName = displayName, Aliases = [gmtId]
			});
		}

		return timezones;
	}

	public List<ExifTimezoneDisplay> GetMovedToDifferentPlaceTimezonesList(DateTime dateTime)
	{
		// Get all system timezones with their offset calculated for the specific date
		// This ensures DST is shown correctly: summer = DST offset, winter = standard offset
		var grouped = TimeZoneInfo.GetSystemTimeZones()
			.Select(tz =>
			{
				// Get the UTC offset for this specific datetime (DST-aware)
				var offset = tz.GetUtcOffset(dateTime);

				// Format offset as +/-HH:mm
				var sign = offset < TimeSpan.Zero ? "-" : "+";
				var absOffset = offset.Duration();
				var offsetString = $"{sign}{absOffset.Hours:D2}:{absOffset.Minutes:D2}";

				// Create display name with actual offset for this date
				var displayName = $"(UTC{offsetString}) {tz.StandardName}";

				return new { tz.Id, DisplayName = displayName };
			})
			.GroupBy(x => x.DisplayName)
			.ToList();

		var timezones = grouped.Select(g => new ExifTimezoneDisplay
		{
			Id = g.First().Id,
			DisplayName = g.Key,
			Aliases = g.Select(x => x.Id).OrderBy(x => x).ToList()
		}).ToList();

		return timezones;
	}
}
