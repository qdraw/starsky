using starsky.foundation.database.Models;
using starsky.foundation.geo.LocationNameSearch.Models;

namespace starsky.foundation.geo.LocationNameSearch.Interfaces;

public interface ILocationNameService
{
	Task<List<GeoNameCity>> SearchCity(string cityName);

	Task<List<CityTimezoneResult>> SearchCityTimezone(string dateTime,
		string cityName);
}
