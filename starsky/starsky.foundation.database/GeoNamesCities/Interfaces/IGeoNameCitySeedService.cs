using System.Threading.Tasks;

namespace starsky.foundation.database.GeoNamesCities.Interfaces;

public interface IGeoNameCitySeedService
{
	Task<bool> Seed();
}
