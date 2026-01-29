using starsky.foundation.database.GeoNamesCities.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoRegionInfo;
using starsky.foundation.geo.LocationNameSearch.Interfaces;
using starsky.foundation.geo.LocationNameSearch.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.geo.LocationNameSearch;

[Service(typeof(ILocationNameService), InjectionLifetime = InjectionLifetime.Singleton)]
public class LocationNameService : ILocationNameService
{
	private readonly IGeoNamesCitiesQuery _query;
	private readonly IGeoNameCitySeedService _seedService;
	private readonly IWebLogger _logger;

	public LocationNameService(IGeoNamesCitiesQuery query,
		IGeoNameCitySeedService seedService, IWebLogger logger)
	{
		_query = query;
		_seedService = seedService;
		_logger = logger;
	}

	public async Task<List<GeoNameCity>> SearchCity(string cityName)
	{
		if ( !await _seedService.Seed() )
		{
			return [];
		}

		return
		[
			..( await _query.Search(cityName, nameof(GeoNameCity.Name),
				nameof(GeoNameCity.AsciiName)) )
			.OrderByDescending(x => x.Population)
		];
	}

	public async Task<List<CityTimezoneResult>> SearchCityTimezone(string dateTime,
		string cityName)
	{
		if ( !await _seedService.Seed() || !DateTime.TryParse(dateTime, out var dateTimeParsed) )
		{
			return [];
		}

		var results = ( await _query.Search(cityName, nameof(GeoNameCity.Name),
				nameof(GeoNameCity.AsciiName)) )
			.OrderByDescending(x => x.Population)
			.ToList();

		var finalResults = new List<CityTimezoneResult>();
		foreach ( var result in results )
		{
			var offset = result.TimeZone.GetUtcOffset(dateTimeParsed);


			var isDaylightSavingTime = result.TimeZone.IsDaylightSavingTime(dateTimeParsed);
			var dstText = isDaylightSavingTime ? "Summer time" : "Winter time";
			if ( !result.TimeZone.SupportsDaylightSavingTime )
			{
				dstText = "No seasonal time change";
			}

			var (locationCountry, _) =
				new RegionInfoHelper(_logger).GetLocationCountryAndCode(result.CountryCode);

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
