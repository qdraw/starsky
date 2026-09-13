using System.Threading.Tasks;
using starsky.feature.geolookup.Models;

namespace starsky.feature.geolookup.Interfaces;

public interface IOsmMapElementsService
{
	Task<OsmMapElementsResult> LookupAsync(double latitude, double longitude);
}