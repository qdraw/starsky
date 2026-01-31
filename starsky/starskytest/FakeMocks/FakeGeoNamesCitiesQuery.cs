using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public sealed class FakeGeoNamesCitiesQuery : IGeoNamesCitiesQuery
{
	public readonly List<GeoNameCity> Cities = [];

	public Task<GeoNameCity?> GetItem(int geoNameId)
	{
		return Task.FromResult(Cities.FirstOrDefault(x => x.GeonameId == geoNameId));
	}

	public Task<List<GeoNameCity>> Search(string search, int maxResults, params string[] fields)
	{
		return Task.FromResult(Cities.Where(x => x.Name.Contains(search)).Take(maxResults)
			.ToList());
	}

	public Task<List<GeoNameCity>> AddRange(List<GeoNameCity> items)
	{
		Cities.AddRange(items);
		return Task.FromResult(items);
	}
}
