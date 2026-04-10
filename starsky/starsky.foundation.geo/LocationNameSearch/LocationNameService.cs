using System.Globalization;
using starsky.foundation.database.GeoNamesCities.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoRegionInfo;
using starsky.foundation.geo.LocationNameSearch.Interfaces;
using starsky.foundation.geo.LocationNameSearch.Models;
using starsky.foundation.geo.TimezoneHelper;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.geo.LocationNameSearch;

[Service(typeof(ILocationNameService), InjectionLifetime = InjectionLifetime.Scoped)]
public class LocationNameService(
	IGeoNamesCitiesQuery query,
	IGeoNameCitySeedService seedService,
	IWebLogger logger)
	: ILocationNameService
{
	public async Task<List<GeoNameCity>> SearchCity(string cityName)
	{
		if ( !await seedService.Seed() )
		{
			return [];
		}

		return
		[
			..( await query.Search(cityName, 10, nameof(GeoNameCity.Name),
				nameof(GeoNameCity.AsciiName)) )
			.OrderByDescending(x => x.Population)
		];
	}
	
	private static bool ParseDateTime(string dateTime, out DateTime dateTimeParsed)
	{
		// Accept both: "2026-06-30T12:00:00" and "2026-02-01T14:15:21.659Z"
		var formats = new[]
		{
			"yyyy-MM-ddTHH:mm:ss",
			"yyyy-MM-ddTHH:mm:ss.fffZ"
		};
		var provider = CultureInfo.InvariantCulture;
		// Always treat as local time, do not convert to UTC
		return DateTime.TryParseExact(dateTime,
			formats,
			provider,
			DateTimeStyles.None,
			out dateTimeParsed);
	}

	public async Task<List<CityTimezoneResult>> SearchCityTimezone(string dateTime,
		string cityName)
	{
		var isDateTimeParsed = ParseDateTime(dateTime, out var dateTimeParsed);

		if ( !await seedService.Seed() || !isDateTimeParsed ||
		     cityName.Length <= 2 )
		{
			return [];
		}

		var results = ( await query.Search(cityName, 10, nameof(GeoNameCity.Name),
				nameof(GeoNameCity.AsciiName)) )
			.OrderByDescending(x => x.Population)
			.ToList();

		var finalResults = new List<CityTimezoneResult>();
		foreach ( var result in results )
		{
			var offset = result.GetTimeZone().GetUtcOffset(dateTimeParsed);
			var isDaylightSavingTime = result.GetTimeZone().IsDaylightSavingTime(dateTimeParsed);
			var dstText = isDaylightSavingTime ? "Summer time" : "Winter time";
			if ( !result.GetTimeZone().HasFutureDst(dateTimeParsed) )
			{
				dstText = "No seasonal time change";
			}

			var (locationCountry, _) =
				new RegionInfoHelper(logger).GetLocationCountryAndCode(result.CountryCode);

			finalResults.Add(new CityTimezoneResult
			{
				Id = result.TimeZoneId,
				AltText = $"UTC({offset.Hours:D2}:{offset.Minutes:D2}) " +
				          $"{locationCountry} {result.Province}, {dstText}",
				DisplayName = result.Name
			});
		}

		return finalResults;
	}
}
