using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Interfaces;

public interface IGeoNamesCitiesQuery
{
	Task<GeoNameCity?> GetItem(int geoNameId);
	Task<GeoNameCity> AddItem(GeoNameCity item);

	Task<List<GeoNameCity>> Search(string search, int maxResults,
		params string[] fields);

	Task<List<GeoNameCity>> AddRange(List<GeoNameCity> items);
}
